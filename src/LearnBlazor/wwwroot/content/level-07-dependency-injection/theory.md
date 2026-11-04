## The one idea

A component should not create the things it depends on (a database, an API client, a cache). It **asks** for them, and the app's **service container** hands them over. You register a service once in `Program.cs`, and inject it wherever you need it.

The hard part in Blazor is not the syntax. It is the **lifetime**: how long one instance lives, and who shares it. That depends on the render mode, and AI-generated code gets it wrong often.

## Register in `Program.cs`

```csharp
// Program.cs
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddTransient<IPriceFormatter, PriceFormatter>();
```

- The first type is what components **ask for** (usually an interface). The second is the **class that is created**.
- Code depends on `ICartService`, not `CartService`, so you can swap the implementation (a fake in tests, a different one in WebAssembly).
- You can also register a class alone (`AddScoped<CartService>()`), or with a factory: `AddScoped(sp => new HttpClient { ... })`.
- Registration must happen **before** `builder.Build()`.

### The three lifetimes

| Lifetime | One instance per... |
| --- | --- |
| `AddSingleton` | the whole app (the process) |
| `AddScoped` | **scope**: see the next section, it depends on the mode |
| `AddTransient` | every injection: always a new instance |

## Inject into a component

In markup, use **`@inject`**:

```razor
@page "/cart"
@inject ICartService Cart
@inject NavigationManager Nav

<p>@Cart.Items.Count items</p>
<button @onclick="@(() => Nav.NavigateTo("/checkout"))">Checkout</button>
```

The syntax is `@inject Type PropertyName`. It creates a property you use like any other.

In a **code-behind** file, use **`[Inject]`** on a property:

```csharp
public partial class Cart
{
    [Inject] public ICartService CartService { get; set; } = default!;
}
```

Since **.NET 9**, components also support **constructor injection**:

```csharp
public partial class Cart(ICartService cartService)
{
    private int Count => cartService.Items.Count;
}
```

Injected services are set **before** `OnInitialized` runs, so you can use them there. They are **not** available in the component's constructor when you use `@inject` or `[Inject]`.

If the service was never registered, the component fails when it is created: *Cannot provide a value for property 'Cart' on type '...'. There is no registered service of type 'ICartService'.*

## What "Scoped" means in each mode

This is the most important part of the level.

| Where the component runs | One scope is... | So a Scoped service is shared by... |
| --- | --- | --- |
| **Static SSR** (no interactivity) | one **HTTP request** | that request only |
| **Interactive Server** | one **circuit** (one browser tab connection) | one user, one tab, while the circuit lives |
| **Interactive WebAssembly** | the app running in the tab | the tab: it acts like a **singleton** |

- In Interactive Server, a Scoped service lives as long as the circuit: across many clicks and page navigations. A new tab (or a page reload) is a new circuit, so a new instance.
- With **prerendering**, the page first renders in the HTTP request scope, then again in the circuit (or in WebAssembly). Each one gets its **own** Scoped instance, so state set during prerender is gone.
- In WebAssembly there is only one user, so Singleton and Scoped behave the same.

### Singleton on the server is shared by everyone

```csharp
builder.Services.AddSingleton<UserState>();   // ❌ on the server
```

On the server, a Singleton is **one object for all users of the app**. If `UserState` holds the current user's cart or name, user B sees user A's data. This is a real data leak, and a classic AI mistake.

Rule of thumb: **per-user state is Scoped**. Singleton is only for things that are safe to share: configuration, a cache of public data, a clock. A shared singleton must also be **thread-safe**, because many circuits call it at the same time.

## Transient and `IDisposable`

The container **tracks** transient services that implement `IDisposable`, and disposes them only when the **scope** ends. In Blazor Server the scope is the circuit, so every transient disposable stays in memory until the user closes the tab. Inject it in a component that renders many times, and memory grows.

- Prefer Scoped for services that hold resources.
- If you need a fresh instance with its own lifetime, use a **factory** (like `IDbContextFactory`), or a component-owned scope (below).

## `OwningComponentBase`: a scope per component

Sometimes you want a Scoped service to live only as long as **one component**, not the whole circuit. Inherit **`OwningComponentBase`**:

```razor
@page "/editor"
@inherits OwningComponentBase<IDraftService>

<p>@Service.Title</p>
```

- `OwningComponentBase<TService>` creates a new scope for the component and exposes the service as **`Service`**.
- The non-generic `OwningComponentBase` exposes **`ScopedServices`**: call `ScopedServices.GetRequiredService<T>()`.
- The scope (and its Scoped and transient services) is **disposed when the component is disposed**.
- Services injected with `@inject` still come from the circuit scope, not from this one.

## `DbContext` in Blazor Server

`AddDbContext` registers `DbContext` as **Scoped**. In Blazor Server that means one `DbContext` per **circuit**: it lives a long time, keeps tracking entities, and is **not thread-safe**. Two quick clicks that query at the same time throw *A second operation was started on this context instance*.

Use a factory and a short-lived context per operation:

```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlServer(conn));
```

```razor
@inject IDbContextFactory<AppDbContext> DbFactory

@code {
    private List<Product> products = [];

    protected override async Task OnInitializedAsync()
    {
        using var db = await DbFactory.CreateDbContextAsync();
        products = await db.Products.ToListAsync();
    }
}
```

When you read AI code with `@inject AppDbContext Db` in an interactive server component, this is the thing to question.

## Auto and WebAssembly: register in both projects

A Blazor Web App with WebAssembly or Auto has **two** `Program.cs` files: the server project and the **`.Client`** project. A component that runs in WebAssembly gets services from the **client** container, which knows nothing about the server's registrations.

```csharp
// Server Program.cs
builder.Services.AddScoped<IWeatherService, ServerWeatherService>();   // reads the database

// Client Program.cs
builder.Services.AddScoped<IWeatherService, HttpWeatherService>();     // calls an API
```

- With **Auto** (or prerendered WebAssembly), the same component runs on the server first and in the browser later, so the service must be registered in **both** places, often with different implementations of one interface.
- Forget the client registration and the component works on the server, then fails in the browser with *There is no registered service of type 'IWeatherService'*.
- The interface lives in the `.Client` project (or a shared library), because the server project references the client project, not the other way round.

## Built-in services

You can inject these without registering them:

| Service | What it does |
| --- | --- |
| `NavigationManager` | the current URL, and `NavigateTo(...)` |
| `IJSRuntime` | calls JavaScript |
| `ILogger<T>` | logging, with the class name as the category |
| `IConfiguration` | settings (on the server; in WebAssembly only `wwwroot/appsettings.json`, which is public) |

`HttpClient` is the exception. The WebAssembly template registers it in the `.Client` `Program.cs`:

```csharp
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```

On the server it is **not** registered by default: add `builder.Services.AddHttpClient()` (and then inject `IHttpClientFactory` or a typed client). Never `new HttpClient()` per call.

## Cheat sheet

- Register: `builder.Services.AddScoped<IService, Impl>();` before `Build()`.
- Inject: `@inject IService Name` in markup, `[Inject]` in code-behind, or a constructor (.NET 9+).
- Scoped = per request (static SSR), per circuit (Server), per tab (WebAssembly).
- Singleton on the server = shared by **all users**. Never put user data in it.
- Transient + `IDisposable` lives until the scope ends: in Server, until the tab closes.
- `OwningComponentBase<T>` = a scope that ends with the component.
- Server + EF Core: `AddDbContextFactory` and `using var db = ...` per operation.
- Auto/WebAssembly: register in the server **and** the `.Client` `Program.cs`.

## Checklist before you move on

- [ ] I can register a service with an interface and pick a lifetime.
- [ ] I can inject a service with `@inject` and with `[Inject]`.
- [ ] I can say what Scoped means in static SSR, Interactive Server and WebAssembly.
- [ ] I can explain why a Singleton with user data leaks data on the server.
- [ ] I know why `IDbContextFactory` is better than `DbContext` in Blazor Server.
- [ ] I know why an Auto component needs its services in both projects.

Now close this tab and go to **Predict**.
