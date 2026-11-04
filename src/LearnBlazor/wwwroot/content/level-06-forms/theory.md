## The one idea

A Blazor form is an **`EditForm`** around a **model object**. Input components bind to the model's properties, a **validator** checks the model with rules (usually **data annotations**), and the form calls **your method** only when the rules pass.

In AI-generated code, find the model, the rules and the submit handler, and you know what the form does.

## A complete form

```razor
@page "/signup"

<EditForm Model="model" OnValidSubmit="Save">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <label>Name <InputText @bind-Value="model.Name" /></label>
    <ValidationMessage For="() => model.Name" />

    <label>Age <InputNumber @bind-Value="model.Age" /></label>

    <button type="submit">Save</button>
</EditForm>

@code {
    private SignupModel model = new();

    private void Save() { /* model is valid here */ }

    public class SignupModel
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public string? Name { get; set; }

        [Range(18, 120)]
        public int Age { get; set; }
    }
}
```

- **`EditForm`** renders a `<form>`. It creates an **`EditContext`** for the model and shares it with everything inside (as a cascading value).
- **`DataAnnotationsValidator`** turns the attributes on the model into validation. **Without it, nothing is validated**, and every submit counts as valid.
- **`ValidationSummary`** shows all the error messages in one list.
- **`ValidationMessage For="() => model.Name"`** shows the errors for one field. `For` takes a **lambda** that points to the property, not a string.
- The button must be **`type="submit"`**.

## Model or EditContext (not both)

```razor
<EditForm Model="model" ...>            @* simple: Blazor creates the EditContext *@
<EditForm EditContext="editContext" ...> @* advanced: you create it yourself *@

@code {
    private EditContext editContext = default!;
    protected override void OnInitialized() => editContext = new EditContext(model);
}
```

Use **`EditContext`** when you need it in code: to call `editContext.Validate()`, to check `editContext.IsModified()`, or to listen to `OnFieldChanged`. Setting **both** `Model` and `EditContext` throws an exception.

## The three submit callbacks

| Callback | When it runs |
| --- | --- |
| `OnValidSubmit` | Submit, and validation **passed** |
| `OnInvalidSubmit` | Submit, and validation **failed** |
| `OnSubmit` | Every submit. **You** decide: call `editContext.Validate()` yourself |

- `OnValidSubmit` and `OnInvalidSubmit` can be used **together**.
- `OnSubmit` can't be combined with either of them: that throws an exception.

## Data annotations

The rules live on the model, from `System.ComponentModel.DataAnnotations`:

```csharp
public class AccountModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
    public string? ConfirmPassword { get; set; }

    [Range(1, 10)]
    public int Quantity { get; set; }
}
```

- **`[Required]`** fails for `null`, and for an **empty or whitespace string**.
- **`[Required]` on a plain `int`** does almost nothing: an `int` always has a value (`0`). To force the user to fill a number, make it **`int?`**.
- **`[StringLength(max, MinimumLength = min)]`** limits the length of a string.
- **`[Range(min, max)]`** limits a number.
- **`[EmailAddress]`** checks the format loosely (basically "has an `@`"). It passes for `null`, so add `[Required]` too.
- **`[Compare("Password")]`** checks that this property equals another one. Put it on the **confirm** field.

## The built-in inputs

All of them bind with **`@bind-Value`** (capital **V**), not `@bind`:

| Component | Renders | Typical type |
| --- | --- | --- |
| `InputText` | `<input>` | `string` |
| `InputTextArea` | `<textarea>` | `string` |
| `InputNumber` | `<input type="number">` | `int`, `decimal`, `double`... |
| `InputDate` | `<input type="date">` | `DateTime`, `DateOnly` |
| `InputCheckbox` | `<input type="checkbox">` | `bool` |
| `InputSelect` | `<select>` | `string`, `enum`, `int`... |
| `InputRadioGroup` + `InputRadio` | radio buttons | `string`, `enum`... |
| `InputFile` | `<input type="file">` | no binding: use `OnChange` |

```razor
<InputSelect @bind-Value="model.Country">
    <option value="">Pick one</option>
    <option value="PT">Portugal</option>
</InputSelect>

<InputRadioGroup @bind-Value="model.Size">
    <InputRadio Value="Size.Small" /> Small
    <InputRadio Value="Size.Large" /> Large
</InputRadioGroup>

<InputFile OnChange="LoadFile" />
```

- The inputs must be **inside an `EditForm`**: they need its `EditContext`, and throw without it.
- `@bind-Value` sets three parameters for you: `Value`, `ValueChanged` and `ValueExpression`. The expression is how the input knows **which field** it is, so validation can mark it.
- **`InputNumber`** shows its own error when the text isn't a number (*"The field Age must be a number."*), even without a validator.
- **`InputFile`** doesn't bind. Its `OnChange` gives you an `InputFileChangeEventArgs`; read a file with `e.File.OpenReadStream(maxAllowedSize)` (the default limit is about 500 KB). It needs an interactive render mode.

## The CSS classes

Each input gets CSS classes from the `EditContext`:

- **`valid`** or **`invalid`**: the field's current state.
- **`modified`**: the user changed the field. An edited bad field gets `modified invalid`.
- Messages render with the class **`validation-message`**. The summary is a `<ul class="validation-errors">`.

## Forms under static SSR

In a page with **no interactive render mode** (static SSR, the default in a Blazor Web App), there is no live connection. Clicking submit sends a real **HTTP POST** to the page. The server renders the page again, reads the posted values into your model, validates it, and calls `OnValidSubmit`.

For this to work, two things are needed:

```razor
@page "/contact"

<EditForm Model="Model" FormName="contact" OnValidSubmit="Send">
    <DataAnnotationsValidator />
    <InputText @bind-Value="Model!.Email" />
    <button type="submit">Send</button>
</EditForm>

@code {
    [SupplyParameterFromForm]
    private ContactModel? Model { get; set; }

    protected override void OnInitialized() => Model ??= new();
}
```

- **`FormName`**: a name that is **unique** on the page. The POST carries it, so the server knows **which form** was submitted. Without it, the POST fails with an error saying the request doesn't specify which form is being submitted.
- **`[SupplyParameterFromForm]`**: on the POST, Blazor fills this property from the form data. On a normal GET it is `null`, so create it with **`Model ??= new()`**. A plain `Model = new()` in `OnInitialized` would throw away the posted values.
- In an **interactive** component, none of this is needed: submit is a normal event, and `FormName` is ignored.

### Enhance

```razor
<EditForm Model="Model" FormName="contact" Enhance OnValidSubmit="Send">
```

**`Enhance`** turns on **enhanced form handling**: Blazor sends the POST with `fetch` and patches the new HTML into the page, so there's no full page reload and the scroll position stays. For a plain `<form>`, the same thing is the `data-enhance` attribute.

## The antiforgery token

A POST could be faked by another site (CSRF). Blazor protects against it with an **antiforgery token**:

- `Program.cs` calls **`app.UseAntiforgery()`** (the template already does).
- **`EditForm` adds the token automatically**, as a hidden field.
- A plain HTML `<form method="post">` needs **`<AntiforgeryToken />`** inside it (and `@formname` for the name). Without the token, the POST is rejected.

## Reading AI code: things to check

- No **`<DataAnnotationsValidator />`**? Then the annotations do nothing.
- **`OnSubmit`** together with **`OnValidSubmit`**? That throws.
- **`Model` and `EditContext`** both set? That throws.
- A static SSR page whose form "does nothing" or errors on submit: look for a missing **`FormName`** or **`[SupplyParameterFromForm]`**.
- **`Model = new();`** in `OnInitialized` of a static SSR form: it wipes the posted values. It should be `Model ??= new();`.
- **`@bind`** on an `InputText`: it should be **`@bind-Value`**.

## Cheat sheet

| You want | Write |
| --- | --- |
| A form for a model | `<EditForm Model="model" OnValidSubmit="Save">` |
| Turn on the rules | `<DataAnnotationsValidator />` |
| All errors / one field | `<ValidationSummary />` / `<ValidationMessage For="() => model.X" />` |
| React to bad input | `OnInvalidSubmit="ShowError"` |
| Static SSR form | `FormName="x"` + `[SupplyParameterFromForm]` + `Model ??= new()` |
| No full page reload | `Enhance` |
| Token on a plain form | `<AntiforgeryToken />` |

## Checklist before you move on

- [ ] I can write an `EditForm` with a validator, a summary and a message for one field.
- [ ] I know when each submit callback runs, and which ones can't be combined.
- [ ] I can put `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]` and `[Compare]` on a model.
- [ ] I can pick the right input component for a `string`, a number, a date, a `bool` and an `enum`.
- [ ] I can explain why a static SSR form needs `FormName` and `[SupplyParameterFromForm]`.

Now close this tab and go to **Predict**.
