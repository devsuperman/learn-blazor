## The one idea

A Blazor app gets its data by **calling a web API** with `HttpClient`, and it decides what a user can see with **authorization**.

Two rules sit under everything in this level:

1. Code that runs **in the browser** (WebAssembly) can't open a database. It asks a server API.
2. Anything that runs in the browser can be **changed by the user**. Hiding a button is not security. The **API** must check who is calling.

When you read AI-generated code, look for these: where the `HttpClient` comes from, what happens when the call fails, and whether the API (not only the page) is protected.

## HttpClient: where it comes from

**WebAssembly** (`Client/Program.cs`): register one `HttpClient` whose base address is the site itself:

```csharp
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```

Then a relative URL like `"api/products"` goes to the same server that served the app.

**Server** (Interactive Server, or the server project): don't `new HttpClient()` yourself. Use **`IHttpClientFactory`**, usually through a **typed client**:

```csharp
builder.Services.AddHttpClient<WeatherClient>(client =>
    client.BaseAddress = new Uri("https://api.example.com/"));
```

```csharp
public class WeatherClient(HttpClient http)
{
    public Task<Forecast[]?> GetAsync() =>
        http.GetFromJsonAsync<Forecast[]>("forecast");
}
```

The factory manages the connections for you. A named client (`AddHttpClient("weather", ...)` then `factory.CreateClient("weather")`) is the other common style.

## Calling the API

The JSON helpers live in `System.Net.Http.Json`:

```csharp
var products = await Http.GetFromJsonAsync<List<Product>>("api/products");

var response = await Http.PostAsJsonAsync("api/products", newProduct);
response.EnsureSuccessStatusCode();
var created = await response.Content.ReadFromJsonAsync<Product>();
```

- **`GetFromJsonAsync<T>`** sends a GET, checks the status, and turns the JSON body into a `T`. It **throws** `HttpRequestException` on a 404 or 500. It returns `T?`: the result can be `null`.
- **`PostAsJsonAsync`** turns your object into JSON and returns the **`HttpResponseMessage`**. It does **not** throw on a 400 or 500. Call **`EnsureSuccessStatusCode()`** or check **`IsSuccessStatusCode`**.

## Loading, errors, and null

A network call takes time and can fail. A good component handles three states:

```razor
@if (error is not null)
{
    <p class="error">@error</p>
}
else if (products is null)
{
    <p>Loading...</p>
}
else
{
    @foreach (var p in products) { <p>@p.Name</p> }
}

@code {
    private List<Product>? products;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            products = await Http.GetFromJsonAsync<List<Product>>("api/products");
        }
        catch (HttpRequestException)
        {
            error = "Could not load the products.";
        }
    }
}
```

- `products` starts as `null`, so the first render shows **Loading...** (the first render happens before the `await` finishes).
- Without `try/catch`, a failed call throws and the circuit or app shows the error UI.
- AI code often writes `products!.Count` or skips the `null` check. That crashes on the first render.

## Why WebAssembly needs a web API

A WebAssembly component runs **in the user's browser**. A connection string there would be visible to everyone, and the browser can't open a database connection anyway. So the data path is:

**browser (WebAssembly) → HTTP → server API → database**

**CORS in one line:** if the API is on a **different origin** (another domain or port), the browser blocks the call unless the **API** allows your origin with CORS headers (`AddCors` / `UseCors` on the API, not in Blazor).

### Auto and WebAssembly components: one service, two sides

An Auto (or prerendered WebAssembly) component runs **first on the server**, then in the browser. If it injects `HttpClient`, the server side may not have one with the right base address. The common fix is an **interface** with two implementations:

```csharp
public interface IProductService { Task<List<Product>> GetAllAsync(); }
```

```csharp
// Client/Program.cs: calls the API
builder.Services.AddScoped<IProductService, HttpProductService>();

// Server/Program.cs: reads the database directly
builder.Services.AddScoped<IProductService, DbProductService>();
```

The component only uses `@inject IProductService Products`. It works on both sides. The server also exposes the API (for example `app.MapGet("/api/products", ...)`) that `HttpProductService` calls.

## Authentication vs authorization

- **Authentication**: *who are you?* Logging in. The result is a **user** (`ClaimsPrincipal`) with **claims** like a name and roles.
- **Authorization**: *what are you allowed to do?* Checking that user against a rule (a role or a policy).

Blazor doesn't log users in by itself; ASP.NET Core does (cookies, Identity, OpenID Connect). Blazor **reads** the result through **`AuthenticationStateProvider`**, a service that returns an `AuthenticationState` with a `User`.

## Getting the auth state into components

Register it once in `Program.cs`:

```csharp
builder.Services.AddCascadingAuthenticationState();
```

This cascades a **`Task<AuthenticationState>`** to every component (older code wraps the app in `<CascadingAuthenticationState>` instead). In code:

```razor
@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private async Task DeleteAsync()
    {
        var authState = await AuthenticationStateTask!;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin"))
        {
            // ...
        }
    }
}
```

It is a **Task**, so you must **`await`** it. `user` is never `null`: an anonymous user is a `ClaimsPrincipal` with `IsAuthenticated == false`.

## AuthorizeView: show or hide markup

```razor
<AuthorizeView>
    <Authorized>Hello, @context.User.Identity?.Name!</Authorized>
    <NotAuthorized><a href="login">Log in</a></NotAuthorized>
    <Authorizing>Checking...</Authorizing>
</AuthorizeView>

<AuthorizeView Roles="Admin,Manager">
    <button>Delete</button>
</AuthorizeView>

<AuthorizeView Policy="CanEdit">
    <button>Edit</button>
</AuthorizeView>
```

- **`Authorized`**: the user passes. **`NotAuthorized`**: they don't. **`Authorizing`**: shown while the auth state is still loading.
- Plain content (no `<Authorized>`) is treated as the authorized content.
- `context` here is the **`AuthenticationState`**.
- `Roles="Admin,Manager"` means **any** of these roles. `Policy` uses a named policy from `AddAuthorization`.

## Protecting pages: [Authorize]

```razor
@page "/admin"
@attribute [Authorize(Roles = "Admin")]
```

- `[Authorize]` only works on **routable components** (with `@page`). On a child component it is **ignored**: use `AuthorizeView` there.
- In interactive routing, the **`AuthorizeRouteView`** in the router does the check:

```razor
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)">
            <NotAuthorized><p>You are not allowed here.</p></NotAuthorized>
        </AuthorizeRouteView>
    </Found>
</Router>
```

A plain `RouteView` ignores `[Authorize]`. If the attribute "does nothing", check the router first.

## The security rule

In WebAssembly, every `AuthorizeView` and `[Authorize]` runs **in the browser**. A user can edit the downloaded code, or just call the API with a tool. So:

- Hiding UI is **user experience**, not security.
- The **API endpoint** must check too: `[Authorize]` on the controller, or `.RequireAuthorization()` on a minimal API.

```csharp
app.MapDelete("/api/products/{id}", DeleteProduct)
   .RequireAuthorization(p => p.RequireRole("Admin"));
```

When AI code hides an "Admin" button but the API has no authorization, that is a **security bug**.

## Checklist before you move on

- [ ] I can register `HttpClient` for WebAssembly, and a typed client on the server.
- [ ] I know which JSON helper throws on an error status, and which doesn't.
- [ ] I can write a component with loading, error and data states.
- [ ] I can explain why a WebAssembly app needs a web API.
- [ ] I can use `AuthorizeView`, `[Authorize]`, and read the user in code.
- [ ] I know why the API must check authorization even when the UI hides a button.

Now close this tab and go to **Predict**.
