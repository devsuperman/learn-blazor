## The one idea

Blazor runs your C# in .NET, but the browser only speaks **JavaScript**. When you need something Blazor doesn't wrap (local storage, the clipboard, a chart library, scrolling), you cross the bridge with **JS interop**: C# calls JavaScript through **`IJSRuntime`**, and JavaScript calls back into .NET through **`DotNet`** and **`DotNetObjectReference`**.

In AI-generated code, JS interop is where many bugs hide: calls made too early, and references never disposed. Know **when** a JS call is allowed and **who cleans up what**.

## Calling JavaScript from C#

Inject **`IJSRuntime`** and call a function by name:

```razor
@inject IJSRuntime JS

<button @onclick="Save">Save</button>

@code {
    private async Task Save()
    {
        await JS.InvokeVoidAsync("localStorage.setItem", "theme", "dark");
        string? theme = await JS.InvokeAsync<string?>("localStorage.getItem", "theme");
    }
}
```

- **`InvokeVoidAsync`**: call a function and ignore its result.
- **`InvokeAsync<T>`**: call a function and get its result, converted from JSON to `T`. Use a nullable type (`string?`) when JS can return `null`.
- Both return a **`ValueTask`**, so you `await` them.
- The **identifier is a path on `window`**: `"localStorage.setItem"` means `window.localStorage.setItem`. `"alert"` means `window.alert`. Your own functions must be reachable from `window` too (for example `window.myApp = { ... }`), unless you use a module (below).
- Global scripts go in `App.razor` (or `index.html` in WebAssembly), **after** the Blazor script tag.

## When can you call JS? Not during prerendering

This is the most common JS interop bug. During **prerendering** (and in **static SSR**), the component runs **on the server** to build HTML. There is no browser yet, so there is no JavaScript to call. A JS call there throws:

> InvalidOperationException: JavaScript interop calls cannot be issued at this time. This is because the component is being statically rendered. When prerendering is enabled, JavaScript interop calls can only be performed during the OnAfterRenderAsync lifecycle method.

```razor
@* ❌ Breaks when prerendered *@
protected override async Task OnInitializedAsync()
{
    theme = await JS.InvokeAsync<string?>("localStorage.getItem", "theme");
}
```

The fix: make JS calls in **`OnAfterRenderAsync`**, which only runs once the component is **interactive** in the browser. Use `firstRender` to do it once, and call `StateHasChanged()` if you change what is shown:

```razor
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        theme = await JS.InvokeAsync<string?>("localStorage.getItem", "theme");
        StateHasChanged();
    }
}
```

- Event handlers (`@onclick`) are safe: they only run when the component is interactive.
- In **static SSR** (no render mode), `OnAfterRenderAsync` never runs, so there is no JS interop.

## JS modules: `import` instead of globals

Functions on `window` pollute the global scope. Prefer a **JavaScript module** with `export`:

```js
// wwwroot/js/clipboard.js
export function copy(text) {
    return navigator.clipboard.writeText(text);
}
```

Load it with the special identifier **`"import"`**. You get back an **`IJSObjectReference`**, and you call the module's exports on it:

```razor
@inject IJSRuntime JS
@implements IAsyncDisposable

@code {
    private IJSObjectReference? module;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/clipboard.js");
        }
    }

    private async Task Copy() => await module!.InvokeVoidAsync("copy", "Hello");

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is gone (user closed the tab). Nothing to clean up.
            }
        }
    }
}
```

- The path is relative to the app's base URL, so `"./js/clipboard.js"` is `wwwroot/js/clipboard.js`.
- On a module reference, the identifier is the **export name** (`"copy"`), not a `window` path.
- An `IJSObjectReference` holds a JS object alive. **Dispose it** with `@implements IAsyncDisposable`.
- In **Blazor Server**, the connection may already be closed when the component is disposed. Then `DisposeAsync` throws **`JSDisconnectedException`**. Catch it and ignore it.

### Collocated JS: `Component.razor.js`

You can put a module **next to the component**: `Components/Pages/Map.razor.js` beside `Map.razor`. The build publishes it as a static file, and you import it with the **path from the project root**:

```csharp
module = await JS.InvokeAsync<IJSObjectReference>(
    "import", "./Components/Pages/Map.razor.js");
```

AI tools often get this path wrong (for example `"./Map.razor.js"` or `"./js/Map.razor.js"`). The import then fails with a 404 in the browser.

## Element references: `@ref` and `ElementReference`

To give JS a specific DOM element, capture it with **`@ref`**:

```razor
<input @ref="nameInput" />

@code {
    private ElementReference nameInput;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await nameInput.FocusAsync();                          // built-in helper
            await JS.InvokeVoidAsync("myApp.selectAll", nameInput); // passed to JS as the element
        }
    }
}
```

- The reference is only set **after the element is rendered**. In `OnInitialized` it is still empty, so use it in `OnAfterRenderAsync` or in an event handler.
- Don't let JS add or remove elements that Blazor renders: the next render can break. Give JS an empty `<div @ref="host"></div>` to own.

## Calling .NET from JavaScript

### Static methods

Mark a **`public static`** method with **`[JSInvokable]`**, then call it from JS with the **assembly name** and the method name:

```csharp
public static class Prices
{
    [JSInvokable]
    public static decimal GetTax(decimal amount) => amount * 0.2m;
}
```

```js
const tax = await DotNet.invokeMethodAsync("MyApp", "GetTax", 100);
```

- The first argument is the **assembly name** (usually the project name), not the class name.
- It returns a **Promise**. `[JSInvokable("calcTax")]` changes the name JS uses.

### Instance methods: `DotNetObjectReference`

To call a method on **this component**, wrap it with **`DotNetObjectReference.Create(this)`** and pass that to JS:

```razor
@implements IDisposable
@inject IJSRuntime JS

<p>Width: @width</p>

@code {
    private DotNetObjectReference<Resize>? objRef;
    private int width;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            objRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("resizeWatcher.start", objRef);
        }
    }

    [JSInvokable]
    public void OnResize(int newWidth)
    {
        width = newWidth;
        StateHasChanged();
    }

    public void Dispose() => objRef?.Dispose();
}
```

```js
window.resizeWatcher = {
    start: (dotNetHelper) => {
        window.addEventListener("resize", () =>
            dotNetHelper.invokeMethodAsync("OnResize", window.innerWidth));
    }
};
```

- The method is **not static**, but it still needs **`[JSInvokable]`** and must be public.
- In JS you call **`invokeMethodAsync`** on the reference, with **no assembly name**.
- Call **`StateHasChanged()`**: a call from JS is not a Blazor event, so the UI doesn't refresh by itself.
- **Dispose the reference**. Otherwise it keeps the component alive in memory (a leak).

## Synchronous calls: `IJSInProcessRuntime`

In **WebAssembly**, .NET and JS run in the same browser tab, so a call can be synchronous:

```csharp
var js = (IJSInProcessRuntime)JS;
string? value = js.Invoke<string?>("localStorage.getItem", "theme");
```

This only works in **WebAssembly**. In Blazor Server, C# runs on the server and JS in the browser, so every call is a network round trip: the cast fails. Default to the async API.

## New in .NET 10

`IJSRuntime` and `IJSObjectReference` gained methods for things that used to need a small wrapper function:

```csharp
// new Date(2025, 0, 1) in JS, returned as a reference
var date = await JS.InvokeConstructorAsync("Date", 2025, 0, 1);

// read and write JS properties
string title = await JS.GetValueAsync<string>("document.title");
await JS.SetValueAsync("document.title", "Orders");
```

## Reading AI code: quick review

- A JS call in `OnInitialized(Async)` or `OnParametersSet(Async)` → breaks with prerendering. Move it to `OnAfterRenderAsync(firstRender)`.
- `IJSObjectReference` or `DotNetObjectReference` with **no dispose** → a leak.
- `DisposeAsync` with no `try/catch (JSDisconnectedException)` → noisy errors on Blazor Server when the tab closes.
- `JS.InvokeAsync<string>("eval", ...)` → avoid. Write a real function or module.
- `(IJSInProcessRuntime)JS` in a component that might run on the server → it will throw.

## Checklist before you move on

- [ ] I can call a JS function with and without a result, and I know the identifier is a path on `window`.
- [ ] I know why JS calls fail during prerendering, and where to put them.
- [ ] I can import a JS module (also a collocated `.razor.js`) and dispose it safely.
- [ ] I know when an `ElementReference` is ready.
- [ ] I can let JS call a static method and a component instance method, and I dispose the reference.

Now close this tab and go to **Predict**.
