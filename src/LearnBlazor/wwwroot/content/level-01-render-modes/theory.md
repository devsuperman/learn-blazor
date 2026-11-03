## The one idea

A **render mode** answers two questions for a Razor component:

1. **Where does it run?** On the server, or in the browser.
2. **Is it interactive?** Do `@onclick`, `@bind` and other events work after the page loads?

The markup in a `.razor` file looks the same in every mode. The render mode decides what happens to it at runtime. When you read AI-generated Blazor code, the first thing to find is the render mode, because it explains most of what the component can and can't do.

> In this level, "Blazor app" means a **Blazor Web App** (`dotnet new blazor`). That is the default template in .NET 10, and it is where render modes apply.

## The four modes

| Mode | Where the C# runs | Interactive? | How the UI updates |
| --- | --- | --- | --- |
| **Static SSR** (no `@rendermode`) | Server, once per request | No | The server sends finished HTML, then forgets the component |
| **Interactive Server** | Server | Yes | Events go to the server over a SignalR (WebSocket) connection, and UI diffs come back |
| **Interactive WebAssembly** | Browser (.NET on WebAssembly) | Yes | Everything happens in the browser, with no round trip |
| **Interactive Auto** | Server first, then browser | Yes | Uses Server while the WebAssembly files download, and WebAssembly on later visits |

### Static SSR (static server-side rendering)

- This is the **default** when a component has no `@rendermode`.
- The server runs the component, produces HTML, sends it, and **disposes the component**. Nothing stays alive.
- So a button with `@onclick` renders, but **clicking it does nothing**: no C# code is listening.
- Good for content pages, SEO, and fast first loads. It is close to Razor Pages and MVC, but uses components.
- It still gets some extras: **enhanced navigation** (link clicks patch the page instead of reloading it), **enhanced form posts**, and **streaming rendering** (`@attribute [StreamRendering]` sends a placeholder first, then the rest when data arrives).

### Interactive Server

```razor
@page "/server"
@rendermode InteractiveServer
```

- The component lives **on the server** in memory, inside a **circuit**: one per browser tab.
- The browser opens a **WebSocket** (SignalR) connection. A click is sent to the server, the server runs your C#, works out what changed (a diff), and sends only that back.
- ✅ Fast start (no .NET download), and full access to server resources (database, file system, secrets).
- ⚠️ Every click is a network round trip (latency), each user uses server memory, and if the connection drops you see the "reconnecting" UI.

### Interactive WebAssembly

```razor
@page "/wasm"
@rendermode InteractiveWebAssembly
```

- The browser downloads the **.NET runtime and your assemblies** (`.wasm` and `.dll` files under `_framework/`), then runs your C# **in the browser**.
- The component must live in the **`.Client` project**, because only that project is sent to the browser.
- ✅ No server round trips for UI events, works offline after loading, and puts no per-user load on the server.
- ⚠️ Slower first load (the download). No direct database access: call an API with `HttpClient`. Everything you ship can be read by the user, so **no secrets** in the client.

### Interactive Auto

```razor
@page "/auto"
@rendermode InteractiveAuto
```

- On the first visit, the component runs as **Interactive Server** (fast start), while the WebAssembly files **download in the background**.
- Once they are cached, **later visits** use **WebAssembly**.
- Because it may run in either place, the component must live in the **`.Client` project**, and its code must work in both environments. For example, get data through an interface or service, not directly from `DbContext`.

## Applying a render mode

There are three ways to apply one:

```razor
@* 1. On the component definition (all usages) *@
@rendermode InteractiveServer

@* 2. On a component instance, from the parent *@
<Counter @rendermode="InteractiveServer" />

@* 3. Globally, in App.razor: the whole app is interactive *@
<HeadOutlet @rendermode="InteractiveServer" />
<Routes @rendermode="InteractiveServer" />
```

The modes must also be **enabled** in the server's `Program.cs`. The template does this for you based on the `-int` option:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MyApp.Client._Imports).Assembly);
```

The `-int` (interactivity) option of `dotnet new blazor` decides this: `None`, `Server`, `WebAssembly` or `Auto`. The `--all-interactive` option applies the mode globally instead of per page.

### Rules that trip people up

- **Children inherit.** A child of an interactive component uses the same mode. You **cannot** switch a child to a *different* interactive mode (for example, a WebAssembly child inside a Server parent is an error).
- **Parameters crossing a render-mode boundary must be serializable.** For example, a static parent passing data to an interactive child. You can't pass a `RenderFragment` (child content) or a delegate across that boundary.
- **Static parents can host interactive islands.** A static SSR page can contain `<Counter @rendermode="InteractiveServer" />`. Only the counter becomes interactive.

## Prerendering

Interactive components are **prerendered by default**:

1. The server first renders the component to **plain HTML** (as in static SSR), so the user sees content immediately and search engines can read it.
2. Then the interactive runtime starts (the circuit, or WebAssembly) and **renders the component again**, this time live.

**The gotcha:** lifecycle methods run **twice**. `OnInitialized{Async}` runs once during prerendering and once when it becomes interactive. If it loads data, you load it twice, and the UI may flicker as the second load replaces the first.

Ways to handle it:

```razor
@* Turn prerendering off for this component *@
@rendermode @(new InteractiveServerRenderMode(prerender: false))
```

Or keep prerendering and **persist the state** from the first render to the second. In .NET 10, this is the `[PersistentState]` attribute:

```razor
@code {
    [PersistentState]
    public List<Product>? Products { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Products ??= await ProductService.GetAsync(); // runs the query only once
    }
}
```

## Which mode am I in?

Since .NET 9, every component can check:

| Property | Example values | Meaning |
| --- | --- | --- |
| `RendererInfo.Name` | `Static`, `Server`, `WebAssembly`, `WebView` | Where this render is happening **right now** |
| `RendererInfo.IsInteractive` | `true` / `false` | Do events work right now? |
| `AssignedRenderMode` | `InteractiveServerRenderMode`, … or `null` | The mode that was **assigned**, even during prerendering |

A common pattern is to disable a button until the component is interactive:

```razor
<button @onclick="Save" disabled="@(!RendererInfo.IsInteractive)">Save</button>
```

During prerendering, `RendererInfo.Name` is `Static` and `IsInteractive` is `false`, even for an `InteractiveServer` component. A moment later it changes to `Server` / `true`.

## Choosing a mode (quick guide)

- **Mostly content, few clicks?** Use static SSR, and add interactive islands where needed.
- **Internal app, needs server data, fast start, users on a good network?** Use Interactive Server.
- **Rich UI with many interactions, offline use, or you want less server load?** Use Interactive WebAssembly.
- **Want a fast first load *and* WebAssembly later?** Use Auto, but your code must run in both places.

## A fun fact about this site

This study site is a **standalone Blazor WebAssembly** app (`dotnet new blazorwasm`), not a Blazor Web App. It has no server at all: GitHub Pages only serves static files. So there are no render modes here. Everything is always WebAssembly and always interactive. Open your browser's dev tools, then the Network tab, and reload: you'll see the `_framework/*.wasm` downloads described above.

## Checklist before you move on

- [ ] I can name the four modes and say where each one runs.
- [ ] I know why a button does nothing on a page without `@rendermode`.
- [ ] I know why `OnInitialized` runs twice, and two ways to fix it.
- [ ] I can find the render mode in a file I didn't write.

Now close this tab and go to **Predict**.
