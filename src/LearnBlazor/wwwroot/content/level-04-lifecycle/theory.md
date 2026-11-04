## The one idea

A component is a C# object, and Blazor calls a fixed set of **lifecycle methods** on it: when it gets its parameters, before it renders, after it renders, and when it is removed. You **override** the ones you need.

When you read AI-generated Blazor code, almost every page loads data in one of these methods. If you know **when** each one runs and **how often**, you can spot the usual bugs: a `NullReferenceException` on the first render, data that doesn't reload, JavaScript called too early, or a timer that keeps running after you leave the page.

## The order of calls

The first time a component is shown:

```
SetParametersAsync          (parameters arrive)
  OnInitialized / OnInitializedAsync      ← once per instance
  OnParametersSet / OnParametersSetAsync  ← every time parameters are set
render
OnAfterRender / OnAfterRenderAsync (firstRender: true)
```

Every time the **parent re-renders** and passes the parameters again:

```
SetParametersAsync
  OnParametersSet / OnParametersSetAsync
render (if ShouldRender() returns true)
OnAfterRender / OnAfterRenderAsync (firstRender: false)
```

When the component is **removed** from the UI: `Dispose` / `DisposeAsync`.

- The sync method runs first, then the async one: `OnInitialized` → `OnInitializedAsync`.
- You don't call these methods yourself. Blazor does. You only override them.

## SetParametersAsync

```razor
@code {
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // runs BEFORE the parameter properties are set
        await base.SetParametersAsync(parameters); // sets them, then calls OnInitialized/OnParametersSet
    }
}
```

This is the entry point. The base version copies the values into your `[Parameter]` properties and then starts the other methods. You rarely override it. If you do, **call `base.SetParametersAsync(parameters)`**, or the properties are never set and the rest of the lifecycle never runs.

## OnInitialized{Async}: once per instance

```razor
@inject ProductService Products

@if (products is null)
{
    <p>Loading...</p>
}
else
{
    @foreach (var p in products) { <p>@p.Name</p> }
}

@code {
    private List<Product>? products;

    protected override async Task OnInitializedAsync()
    {
        products = await Products.GetAllAsync();
    }
}
```

- Runs **once** for each component instance, after the first parameters are set. Parameters already have their values here.
- This is the usual place to **load data** and **subscribe to events**.
- **With prerendering, it runs twice.** The server prerenders the page with one instance, then the interactive circuit (or WebAssembly) creates a **new** instance and runs it again. So the data is loaded twice, and you may see it "flash". (Level 1 covered prerendering; Level 8 shows how to keep the prerendered data.)

### The render before the await finishes

This is the most important rule of the async methods:

> When `OnInitializedAsync` (or `OnParametersSetAsync`) hits an `await` that is **not finished yet**, Blazor **renders the component right away**, with the data it has. When the task finishes, it renders again.

So on the first render, `products` is still `null`. Without the `@if (products is null)` check, `@foreach (var p in products)` throws a **`NullReferenceException`**. This is one of the most common bugs in AI-generated pages. The fix is always the same: start with `null`, show a **loading state**, and check for `null` in the markup.

## OnParametersSet{Async}: every time parameters are set

```razor
@code {
    [Parameter] public int ProductId { get; set; }
    private Product? product;

    protected override async Task OnParametersSetAsync()
    {
        product = await Products.GetAsync(ProductId);
    }
}
```

- Runs after `OnInitialized` the first time, and **again every time the parent passes parameters**.
- Use it when the work **depends on a parameter** that can change. A classic example: a page `/product/{Id:int}` with links to other products. Blazor **reuses the same instance** when only the route parameter changes, so `OnInitialized` does **not** run again. Code that loads in `OnInitializedAsync` keeps showing the old product.
- It can run even if nothing changed (an object parameter is passed again). If the work is expensive, keep the last value in a field and compare.

## OnAfterRender{Async}(bool firstRender)

```razor
@inject IJSRuntime JS

<input @ref="searchBox" />

@code {
    private ElementReference searchBox;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await searchBox.FocusAsync();
            // or: await JS.InvokeVoidAsync("myLib.init");
        }
    }
}
```

- Runs **after** each render, when the elements exist in the browser. `firstRender` is `true` only the first time.
- This is **the place for JS interop** that touches the page: focusing an element, starting a JS library, reading `localStorage`. (Level 9 goes deeper into JS interop.)
- It is **not called during prerendering or static SSR**. There is no browser yet, only HTML being written on the server. A page with no interactive render mode never gets this call.
- `@ref` fields are only set **after** the render, so they are empty in `OnInitialized`.
- Changing a field here does **not** re-render by itself. If you need to show the new value, call `StateHasChanged()`, and guard it with `if (firstRender)`, or you get an **endless render loop**.

## StateHasChanged and ShouldRender

`StateHasChanged()` tells Blazor: "my state changed, render me again." Blazor already calls it for you after lifecycle methods and after event handlers (Level 3). You call it yourself when the change comes from **outside** Blazor: a timer, a service event, or after `OnAfterRenderAsync` loaded something.

```razor
@code {
    private void OnCartChanged() => InvokeAsync(StateHasChanged);
}
```

A timer or a service event can run on another thread. **`InvokeAsync(StateHasChanged)`** runs it on the component's own context, which is required on the server.

`ShouldRender()` lets a component skip re-renders:

```razor
@code {
    protected override bool ShouldRender() => dataChanged;
}
```

If it returns `false`, the re-render is skipped. It is **not** called for the first render, so the component always renders once.

## Dispose: clean up

When a component is removed (you navigate away, or an `@if` hides it), Blazor calls `Dispose`, if the component implements `IDisposable`. Use **`@implements`**:

```razor
@implements IDisposable
@inject CartService Cart

<p>Items: @Cart.Count</p>

@code {
    protected override void OnInitialized() => Cart.Changed += OnCartChanged;

    private void OnCartChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Cart.Changed -= OnCartChanged;
}
```

Clean up everything that would outlive the component:

- **Event subscriptions** (`+=`) to a longer-lived service: unsubscribe with `-=`. Otherwise the service keeps the component alive (a memory leak) and calls a dead component.
- **Timers**: dispose them, or they keep firing forever.
- **`CancellationTokenSource`**: cancel and dispose it, so a slow request stops when the user leaves.

```razor
@implements IDisposable

@code {
    private readonly CancellationTokenSource cts = new();

    protected override async Task OnInitializedAsync()
    {
        data = await Http.GetFromJsonAsync<Data>("api/data", cts.Token);
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }
}
```

For async cleanup, use **`@implements IAsyncDisposable`** and `public async ValueTask DisposeAsync()`. If a component implements **both**, Blazor only calls `DisposeAsync`.

## Reading AI code: what to look for

- Data loaded in `OnInitializedAsync`, but markup with no `null` check or loading state: crash on the first render.
- Data that depends on a `[Parameter]` (or a route parameter) loaded in `OnInitializedAsync`: it won't reload when the parameter changes. It belongs in `OnParametersSetAsync`.
- `IJSRuntime` called in `OnInitializedAsync`: fails during prerendering. It belongs in `OnAfterRenderAsync` with `if (firstRender)`.
- `StateHasChanged()` in `OnAfterRender` with no `firstRender` guard: endless loop.
- `+=` on a service event, or a `new Timer(...)`, with no `@implements IDisposable`: a leak.
- A timer callback that calls `StateHasChanged()` directly instead of `InvokeAsync(StateHasChanged)`.

## Cheat sheet

| Method | When | How often | Typical use |
| --- | --- | --- | --- |
| `SetParametersAsync` | Parameters arrive | Every time | Rarely overridden |
| `OnInitialized{Async}` | First parameters set | Once per instance (twice with prerendering) | Load data, subscribe |
| `OnParametersSet{Async}` | After parameters are set | Every time | Work that depends on parameters |
| `OnAfterRender{Async}` | After a render | Every render (not when prerendering) | JS interop, `@ref` |
| `ShouldRender` | Before a re-render | Every re-render | Skip renders |
| `Dispose{Async}` | Component removed | Once | Unsubscribe, stop timers, cancel |

## Checklist before you move on

- [ ] I can list the lifecycle methods in the order they run.
- [ ] I know why `OnInitialized` runs twice with prerendering.
- [ ] I can write a loading state that avoids the `null` crash.
- [ ] I know when to use `OnParametersSetAsync` instead of `OnInitializedAsync`.
- [ ] I know why JS interop goes in `OnAfterRenderAsync` with `firstRender`.
- [ ] I can implement `IDisposable` to unsubscribe from an event and stop a timer.

Now close this tab and go to **Predict**.
