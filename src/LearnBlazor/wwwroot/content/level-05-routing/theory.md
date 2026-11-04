## The one idea

A **page** is just a component with a **`@page`** directive. The **`Router`** reads the URL, finds the component whose route template matches, and renders it inside the layout. Values in the URL (like the `42` in `/products/42`) flow into the page as **parameters**.

When you read AI-generated Blazor code, the `@page` lines tell you the app's URLs, and `NavigationManager` and `NavLink` tell you how the user moves between them.

## `@page`: giving a component a URL

```razor
@* Products.razor *@
@page "/products"
@page "/catalog"

<h1>Products</h1>
```

- The route is a **string that starts with `/`**.
- A component can have **several `@page` lines**. Each one is another URL for the same component.
- Without `@page`, the component has no URL (Level 2). You can only use it as a tag.
- Route matching **ignores case**: `/Products` also opens this page.

## The Router

In a Blazor Web App, `App.razor` renders `<Routes />`, and `Routes.razor` holds the router:

```razor
@* Routes.razor (.NET 10 template) *@
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

- **`AppAssembly`**: where to look for components with `@page`. Pages in another project (a Razor class library, or the `.Client` project) are added with `AdditionalAssemblies`.
- **`Found`**: what to render when a route matches. `RouteView` renders the page inside its layout.
- **`NotFoundPage`** (new in .NET 10): the component to show when **no route matches**. Older code uses a `<NotFound>...</NotFound>` block inside the `Router` instead.
- **`FocusOnNavigate`** moves focus to the first `<h1>` after navigation, for screen readers.

In .NET 10 a page can also say "this item doesn't exist" itself, by calling **`Navigation.NotFound()`**. Blazor then shows the not-found page (and returns a 404 status during static server rendering).

## Route parameters

Put a name in **braces** in the route, and add a **`[Parameter]`** property with the **same name**:

```razor
@page "/products/{Id:int}"

<h1>Product @Id</h1>

@code {
    [Parameter] public int Id { get; set; }
}
```

- The name matching **ignores case**: `{id}` fills `Id`.
- `{Id:int}` is a **route constraint**. `/products/42` matches; `/products/abc` does **not** match this route at all, so the user gets the not-found page.
- Without a constraint, the parameter must be a **`string`**. An AI tool that writes `{id}` with an `int` property gets an error at runtime when the value is converted.

### Constraints

| Constraint | Example | Matches |
| --- | --- | --- |
| `int` | `{id:int}` | `42`, `-1` |
| `long` | `{ticks:long}` | `123456789012` |
| `bool` | `{active:bool}` | `true`, `FALSE` |
| `datetime` | `{date:datetime}` | `2026-11-04` |
| `decimal`, `double`, `float` | `{price:decimal}` | `9.99` |
| `guid` | `{key:guid}` | `cd2c1638-1638-72d5-1638-deadbeef1638` |

A constraint checks the **type** of the segment and converts it. Range checks ("id must be positive") belong in your code, not in the route.

`datetime`, `decimal`, `double` and `float` are read with the **invariant culture**, so use `2026-11-04` and `9.99`, not local formats.

### Optional and catch-all

```razor
@page "/orders/{Status?}"
@page "/files/{*Path}"

@code {
    [Parameter] public string? Status { get; set; }   // null for "/orders"
    [Parameter] public string? Path { get; set; }     // "docs/2026/a.pdf"
}
```

- **`{Status?}`** is **optional**: `/orders` and `/orders/open` both match. Optional parameters must come **last**. With a constraint, write `{page:int?}` and use `int?`.
- **`{*Path}`** is a **catch-all**: it takes the rest of the URL, slashes included. It must be the **last** segment, and the property must be a `string`.

## Query strings

For `/search?q=blazor&page=2`, use **`[SupplyParameterFromQuery]`**:

```razor
@page "/search"

<p>Results for "@Q", page @Page</p>

@code {
    [SupplyParameterFromQuery] public string? Q { get; set; }
    [SupplyParameterFromQuery(Name = "page")] public int? Page { get; set; }
}
```

- Put it on the **page** (the component with `@page`), and pass the value down to child components as normal parameters.
- The query name matches the **property name** (ignoring case), or the **`Name`** you give.
- A missing value gives the **default** (`null` for `string?` and `int?`). Use **nullable** types so "missing" is clear.
- Supported types: `string`, `bool`, `int`, `long`, `decimal`, `double`, `float`, `DateTime`, `Guid`, their nullable versions, and **arrays** of them (`?tag=a&tag=b`).

## NavigationManager

Inject it to read the URL or move to another one from code:

```razor
@inject NavigationManager Navigation

<button @onclick="GoToOrder">Open order</button>

@code {
    private void GoToOrder() => Navigation.NavigateTo("orders/7");
}
```

| Member | What it does |
| --- | --- |
| `Uri` | The full current URL: `https://site.com/app/orders/7?x=1` |
| `BaseUri` | The app's base URL, **always ending in `/`**: `https://site.com/app/` |
| `ToBaseRelativePath(uri)` | `Uri` without the base: `orders/7?x=1` |
| `NavigateTo(url)` | Go to the URL inside the app (no full page reload) |
| `NavigateTo(url, forceLoad: true)` | A **full page reload** from the server |
| `NavigateTo(url, replace: true)` | Replace the current history entry, so **Back** doesn't return here |
| `GetUriWithQueryParameter("page", 3)` | Returns the current URL with `page=3` added or changed (it doesn't navigate) |
| `NotFound()` | (.NET 10) Show the not-found page for the current URL |
| `LocationChanged` | An event raised after each navigation |

- A URL **without** a leading `/` (`"orders/7"`) is relative to `BaseUri`. That's what you want when the app is hosted in a sub-folder, like GitHub Pages.
- Use `forceLoad: true` for URLs that are **not** Blazor pages (a server endpoint like `/logout`, a file download) or another site.
- To change one query value: `Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("page", 3));`
- `GetUriWithQueryParameter("page", (int?)null)` **removes** the parameter.

## NavLink

`NavLink` is an `<a>` that adds a CSS class when its URL is the **current** one. Menus use it.

```razor
<NavLink href="" Match="NavLinkMatch.All">Home</NavLink>
<NavLink href="orders">Orders</NavLink>
<NavLink href="admin" ActiveClass="selected">Admin</NavLink>
```

- The class is **`active`** by default. `ActiveClass` changes it.
- **`NavLinkMatch.Prefix`** (the default): active when the URL **starts with** the link's path. `orders` is active on `/orders`, `/orders/7` and `/orders/7/edit`.
- **`NavLinkMatch.All`**: active only when the whole URL matches. The **Home** link needs it: with `Prefix`, `href=""` would match **every** page, so Home would always look active. That's a common bug in AI-generated menus.
- Other attributes (`class`, `target`, ...) pass through to the `<a>`. If you only need a link, a plain `<a href="orders">` works too; Blazor intercepts it and navigates without a reload.

## Same page, new parameters

Going from `/products/1` to `/products/2` renders the **same component type** in the same place. Blazor does **not** create a new instance. It only sets the parameters again.

```razor
@page "/products/{Id:int}"

<h1>@product?.Name</h1>
<a href="products/@(Id + 1)">Next</a>

@code {
    [Parameter] public int Id { get; set; }
    private Product? product;

    // ❌ OnInitializedAsync runs once: "Next" changes the URL, but not the product.
    // ✅ OnParametersSetAsync runs every time the parameters are set.
    protected override async Task OnParametersSetAsync()
    {
        product = await Catalog.GetAsync(Id);
    }
}
```

This is the lifecycle from Level 4: **`OnInitialized{Async}` runs once per instance**, **`OnParametersSet{Async}` runs on every parameter change**. Loading data based on a route or query parameter belongs in **`OnParametersSet{Async}`**. AI code that loads it in `OnInitializedAsync` works until the user clicks a link to the same page with a different id.

## Reading AI code: quick checks

- Does every typed route parameter (`int`, `Guid`, ...) have a **matching constraint** (`{id:int}`)?
- Is the **Home** `NavLink` using `NavLinkMatch.All`?
- Is data loaded from a route parameter in **`OnParametersSet{Async}`**, not only in `OnInitialized{Async}`?
- Does `NavigateTo` use `forceLoad: true` only when it really needs a full reload?

## Checklist before you move on

- [ ] I can give a page two URLs and read its route parameters.
- [ ] I know what happens when a URL breaks a constraint like `{id:int}`.
- [ ] I can write optional (`{x?}`) and catch-all (`{*x}`) parameters.
- [ ] I can read a query value with `[SupplyParameterFromQuery]`.
- [ ] I know `NavigateTo` with `forceLoad` and `replace`, and what `GetUriWithQueryParameter` returns.
- [ ] I know why the Home `NavLink` needs `NavLinkMatch.All`.
- [ ] I know why route-based data loads in `OnParametersSet{Async}`.

Now close this tab and go to **Predict**.
