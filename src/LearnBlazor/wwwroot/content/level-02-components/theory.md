## The one idea

A Blazor UI is a **tree of components**. A component is a `.razor` file: markup plus C#. A parent uses a child like an HTML tag, and passes data **down** to it through **parameters**.

When you read AI-generated Blazor code, most files are components that take parameters and render other components. If you can read a component's parameters, you know how it is meant to be used.

## Anatomy of a component

```razor
@* Greeting.razor *@
<h3>Hello, @Name!</h3>
<p>You have @Count new messages.</p>

@code {
    [Parameter] public string Name { get; set; } = "";
    [Parameter] public int Count { get; set; }
}
```

- The **file name is the class name**: `Greeting.razor` compiles to a C# class called `Greeting`. It must start with an **uppercase** letter: the compiler rejects a lowercase name like `greeting.razor`, and a lowercase tag is always treated as an HTML element.
- The **markup** at the top is rendered. `@Name` writes a C# value into the HTML (HTML-encoded, so it's safe).
- The **`@code { }` block** holds fields, properties and methods. It is the body of the class.
- A component with **no `@page`** is not a page. It has no URL. You use it inside other components.

## Using a component

```razor
@* Inbox.razor *@
@page "/inbox"

<Greeting Name="Ana" Count="3" />
<Greeting Name="@user.Name" Count="@unread" />
```

- Use the class name as a **tag**. Set parameters as **attributes**.
- A **string** parameter takes plain text (`Name="Ana"`). For a C# value, use `@` (`Name="@user.Name"`).
- For non-string parameters, the value is C#: `Count="3"` passes the number `3`. `Count="@unread"` and `Count="unread"` both pass the variable (writing the `@` is clearer).
- If the component lives in another folder (namespace), add `@using MyApp.Components.Shared` to the file, or once in **`_Imports.razor`**, which applies to every component in that folder and below.
- **Missing `@using`?** The tag is rendered as an unknown HTML element (`<greeting>` in the page), and the compiler only shows a **warning** (`RZ10012`). If a component "doesn't show up", check this first.

## Parameters

A parameter is a **public property** with the **`[Parameter]`** attribute.

```razor
@code {
    [Parameter] public string Title { get; set; } = "Untitled";   // default value
    [Parameter, EditorRequired] public Product Product { get; set; } = default!;
}
```

- **Without `[Parameter]`**, the property can't be set from the parent. Setting it as an attribute is an error at runtime ("does not have a property matching the name").
- A **default value** is used when the parent doesn't set the parameter.
- **`[EditorRequired]`** makes the compiler **warn** (not fail) when a parent forgets the parameter. It doesn't make it required at runtime.
- Parameters flow **one way: parent → child**. When the parent re-renders, it sets the child's parameters again.
- One shortcut: if **every** parameter of the child is a simple type (`string`, `int`, `bool`, …) and **none changed**, Blazor skips the child. As soon as one changes, or a parameter is an object or a `RenderFragment`, **all** of them are set again.

### Don't write to your own parameters

```razor
@* ❌ Toggle.razor *@
<button @onclick="() => IsOn = !IsOn">@(IsOn ? "On" : "Off")</button>

@code {
    [Parameter] public bool IsOn { get; set; }
}
```

This seems to work, but the next time the **parent re-renders** and passes the parameters again, its value **overwrites** your change. The toggle "resets" for no visible reason, and only sometimes (see the shortcut above), which makes it hard to debug. It's one of the most common bugs in AI-generated components.

The fix: copy the parameter into a **private field** and change the field, or tell the parent to change it (with an `EventCallback`, which is Level 3).

```razor
@code {
    [Parameter] public bool InitialOn { get; set; }
    private bool isOn;

    protected override void OnInitialized() => isOn = InitialOn;
}
```

## Child content

A component can render markup that the **parent** provides between its tags. Name a `RenderFragment` parameter **`ChildContent`**:

```razor
@* Card.razor *@
<div class="card">
    <h3>@Title</h3>
    @ChildContent
</div>

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

```razor
<Card Title="Tip">
    <p>Drink <strong>water</strong>.</p>
</Card>
```

- The name **must be `ChildContent`** for the content between the tags to go there. Any other name won't receive it.
- `@ChildContent` renders it. If it is `null` (nothing between the tags), nothing renders.

### More than one slot

Use several `RenderFragment` parameters, and set them with **child elements** named after the parameters:

```razor
@* Panel.razor *@
<header>@Header</header>
<main>@Body</main>

@code {
    [Parameter] public RenderFragment? Header { get; set; }
    [Parameter] public RenderFragment? Body { get; set; }
}
```

```razor
<Panel>
    <Header><h2>Orders</h2></Header>
    <Body><p>No orders yet.</p></Body>
</Panel>
```

Once you use named slots like this, plain content between the tags is not allowed. Put it in `<ChildContent>...</ChildContent>` instead.

### Templates: `RenderFragment<T>` and `@typeparam`

A **template** is a render fragment that receives a value. This is how list and grid components let the parent decide how each item looks:

```razor
@* ItemList.razor *@
@typeparam TItem

<ul>
    @foreach (var item in Items)
    {
        <li>@ItemTemplate(item)</li>
    }
</ul>

@code {
    [Parameter, EditorRequired] public IEnumerable<TItem> Items { get; set; } = [];
    [Parameter, EditorRequired] public RenderFragment<TItem> ItemTemplate { get; set; } = default!;
}
```

```razor
<ItemList Items="products">
    <ItemTemplate>
        <strong>@context.Name</strong> · @context.Price.ToString("C")
    </ItemTemplate>
</ItemList>
```

- **`@typeparam TItem`** makes the component **generic**. The type is usually inferred from `Items`.
- Inside the template, the value is called **`context`**. Rename it with `Context="product"` on the template element, which you need when two templates are nested.

## Attribute splatting

To pass any extra HTML attributes (like `class`, `id`, `aria-*`) through to an element, capture the **unmatched** ones:

```razor
@* MyInput.razor *@
<input class="my-input" @attributes="Extra" />

@code {
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? Extra { get; set; }
}
```

```razor
<MyInput placeholder="Search" maxlength="50" />
```

Any attribute that doesn't match a parameter goes into `Extra`, and `@attributes` puts them on the `<input>`. Attributes written **after** `@attributes` win over the captured ones; attributes written **before** can be overwritten by them.

## Code-behind (partial class)

The `@code` block can live in a separate C# file. A `.razor` file compiles to a **partial class**, so you add the other half:

```csharp
// Greeting.razor.cs
namespace MyApp.Components;

public partial class Greeting
{
    [Parameter] public string Name { get; set; } = "";
}
```

The namespace and class name must match the `.razor` file. AI-generated code often uses this style for longer components. To inject services in code-behind, use `[Inject]` on a property instead of `@inject`.

## Checklist before you move on

- [ ] I know the name rules for a component file, and why a lowercase name doesn't render.
- [ ] I can declare a parameter, give it a default, and mark it `[EditorRequired]`.
- [ ] I know why a component should not change its own parameters.
- [ ] I can write a component with `ChildContent`, and one with two named slots.
- [ ] I can read a `RenderFragment<T>` template and say what `context` is.

Now close this tab and go to **Predict**.
