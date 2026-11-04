## The one idea

**State** is the data your UI shows and changes: the current user, a theme, the items in a cart. This level answers two questions:

1. **How does state reach a component** that is deep in the tree, without passing it through every parameter on the way?
2. **Where does state live**, and when is it lost?

When you read AI-generated Blazor code, look for three things: `[CascadingParameter]` (data from an ancestor), an injected `*State` service with an `OnChange` event (shared state), and `PersistentComponentState` or `[PersistentState]` (data kept across prerendering).

## Cascading values

Parameters go one level down (Level 2). A **cascading value** goes to **every descendant**, at any depth:

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<CascadingValue Value="theme">
    @Body
</CascadingValue>

@code {
    private Theme theme = new() { ButtonClass = "btn-dark" };
}
```

```razor
@* ThemedButton.razor (anywhere below the layout) *@
<button class="btn @Theme?.ButtonClass">@ChildContent</button>

@code {
    [CascadingParameter] public Theme? Theme { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

- **`[CascadingParameter]`** receives the value. It is matched **by type**: the closest ancestor `CascadingValue` with a `Theme` value wins.
- If nothing matches, the property stays **`null`**. There is **no error**, so make it nullable and check it.
- The parent does **not** write `Theme="..."` on `<ThemedButton>`. The value arrives on its own.

### `Name` and `IsFixed`

```razor
<CascadingValue Value="userName" Name="CurrentUser" IsFixed="true">
    @Body
</CascadingValue>
```

```razor
@code {
    [CascadingParameter(Name = "CurrentUser")] public string? UserName { get; set; }
}
```

- **`Name`** matches by name instead of type. Use it when you cascade two values of the same type (two `string`s, for example).
- **`IsFixed="true"`** says the value **never changes**. Blazor then doesn't track who uses it, which is cheaper when many components read it. Don't use it for a value that changes.
- When the component that holds a non-fixed `<CascadingValue>` re-renders, the components that use the value **re-render** too, so they see the new value.

### Root-level cascading values

Instead of a `<CascadingValue>` in the layout, you can register the value for the **whole app** in `Program.cs`:

```csharp
builder.Services.AddCascadingValue(sp => new Theme { ButtonClass = "btn-dark" });
builder.Services.AddCascadingValue("AppName", sp => "Learn Blazor");   // named
```

`builder.Services.AddCascadingAuthenticationState()` (Level 10) is the same idea. For a root-level value that changes, wrap it in a `CascadingValueSource<T>` and call `NotifyChangedAsync()`.

### The render mode boundary

Remember the **interactive islands** from Level 1: a static SSR page can contain one component with `@rendermode InteractiveServer`. That component starts a **new, separate tree**. Two things don't cross into it:

- **Cascading values** from a `<CascadingValue>` in a static layout. Once the island is interactive, its `[CascadingParameter]` is **`null`**.
- **Non-serializable parameters**, like a `RenderFragment` (`ChildContent`). Parameters into an island must be serializable (JSON), and a render fragment is code, not data. Blazor throws.

The fixes: register a **root-level** cascading value (in the server `Program.cs`, and also in the `.Client` one for WebAssembly), or make the **whole app** interactive (`<Routes @rendermode="InteractiveServer" />`), so the layout and the page share one tree.

## A state container service

For state that **several unrelated components** share (a cart, filters, a wizard), AI tools usually write a **state container**: a plain class, registered as a service.

```csharp
public class CartState
{
    public List<string> Items { get; } = [];

    public event Action? OnChange;

    public void Add(string item)
    {
        Items.Add(item);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
```

```csharp
builder.Services.AddScoped<CartState>();   // one per user (circuit), see Level 7
```

Any component can `@inject CartState Cart` and call `Cart.Add(...)`. But a component that **shows** the cart must also **listen**:

```razor
@* CartCount.razor *@
@implements IDisposable
@inject CartState Cart

<span>Cart: @Cart.Items.Count</span>

@code {
    protected override void OnInitialized() => Cart.OnChange += OnCartChanged;

    private void OnCartChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Cart.OnChange -= OnCartChanged;
}
```

- **Subscribe** in `OnInitialized`. Without it, the count only updates when this component re-renders for some other reason.
- **`InvokeAsync(StateHasChanged)`**, not plain `StateHasChanged()`: the event is raised by **another** component (or a background task), and `InvokeAsync` runs the re-render on this component's own dispatcher.
- **Unsubscribe** in `Dispose`. The service lives longer than the component; if you forget, the service keeps the dead component alive (a memory leak) and calls it after it is gone.
- **Scoped**, not Singleton, on the server: a singleton would share one cart between **all users**.

## Where state lives, and when it is lost

| Where | Lost when |
| --- | --- |
| A **field** in a component | The component is disposed, for example when you **navigate** away |
| A **scoped service** on Interactive Server (the circuit) | The page is **refreshed**, the tab closes, or a **reconnect fails** |
| Memory on **WebAssembly** (fields, services) | The page is **refreshed** or the tab closes |
| **Browser storage** (`localStorage`, `sessionStorage`) | The user clears it (`sessionStorage`: the tab closes) |
| A **database**, through an API | Only when you delete it |

To keep state across a refresh, save it in the browser or on the server. On **Interactive Server**, use `ProtectedLocalStorage` or `ProtectedSessionStorage` (they encrypt the value with ASP.NET Core Data Protection):

```razor
@using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage
@inject ProtectedSessionStorage Session

@code {
    private int count;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        var result = await Session.GetAsync<int>("count");
        count = result.Success ? result.Value : 0;
        StateHasChanged();
    }
}
```

They use JS interop, so they **don't work during prerendering**: call them in `OnAfterRenderAsync`, not `OnInitializedAsync`. On WebAssembly, there is no Data Protection: call `localStorage` through JS interop (Level 9), and never store secrets there.

## Persisting prerendered state

Level 1 and Level 4: with prerendering, an interactive component runs **twice**. First on the server to produce HTML, then again when it becomes interactive. So `OnInitializedAsync` runs twice, the data is **fetched twice**, and the page can flicker (data, then "Loading...", then data again). This is the **double fetch** problem.

The fix is to save the data from the prerender **into the page**, and read it back on the second run.

### .NET 10: `[PersistentState]`

```razor
@page "/products"
@rendermode InteractiveServer
@inject ProductService ProductService

@code {
    [PersistentState]
    public List<Product>? Products { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Products ??= await ProductService.GetAllAsync();
    }
}
```

- The property must be **public**. Blazor saves it as JSON at the end of the prerender and restores it **before** `OnInitializedAsync` on the interactive run.
- `??=` fetches only when nothing was restored.

### The older API: `PersistentComponentState`

You will see this in a lot of AI code (it works since .NET 8):

```razor
@implements IDisposable
@inject PersistentComponentState AppState

@code {
    private List<Product>? products;
    private PersistingComponentStateSubscription subscription;

    protected override async Task OnInitializedAsync()
    {
        subscription = AppState.RegisterOnPersisting(Persist);

        if (!AppState.TryTakeFromJson<List<Product>>("products", out var saved))
            products = await ProductService.GetAllAsync();
        else
            products = saved;
    }

    private Task Persist()
    {
        AppState.PersistAsJson("products", products);
        return Task.CompletedTask;
    }

    public void Dispose() => subscription.Dispose();
}
```

- **`RegisterOnPersisting`** runs your callback when the prerender ends. **`PersistAsJson`** writes the value.
- **`TryTakeFromJson`** returns `true` (and the value) on the interactive run if something was saved.
- The saved state is **in the HTML**, so the user can read it: don't put secrets in it.

## Checklist before you move on

- [ ] I can provide a value with `<CascadingValue>` and read it with `[CascadingParameter]`, by type and by `Name`.
- [ ] I know what `IsFixed` does, and how to register a root-level cascading value.
- [ ] I know why a cascading value or a `RenderFragment` doesn't reach an interactive island.
- [ ] I can write a scoped state container with `OnChange`, and a component that subscribes and unsubscribes.
- [ ] I can say where state is lost on navigation, refresh, and a failed reconnect.
- [ ] I can fix the double fetch with `[PersistentState]` or `PersistentComponentState`.

Now close this tab and go to **Predict**.
