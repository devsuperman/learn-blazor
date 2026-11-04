## The one idea

Data goes **down** as values, and the user's actions come **back up** as **events**. An event handler changes some C# state, and then Blazor **re-renders** the component to show it.

`@bind` is only a shortcut for "show this value, and update it when an event fires". When you read AI-generated code, ask two questions about every input or button: **which event** updates the state, and **which component** re-renders after it.

## Events need an interactive render mode

`@onclick`, `@bind` and every other event only work when the component is **interactive** (Level 1). In a Blazor Web App, a page with no `@rendermode` uses **static SSR**: the button renders, but clicking it does **nothing**, and there is **no error**. If "the button does nothing", check the render mode first. (A standalone Blazor WebAssembly app, like this study site, is always interactive.)

## One-way: `value="@x"`

```razor
<input value="@name" />
<p>Hello, @name</p>

@code {
    private string name = "Ana";
}
```

This only **shows** `name`. When the user types, the `<input>` changes, but the field `name` does **not**. Nothing is listening.

## Two-way: `@bind`

```razor
<input @bind="name" />
<p>Hello, @name</p>
```

`@bind` writes the value into the element **and** updates the field when the element raises an event. For an `<input>`, that event is **`onchange`** by default: it fires when the input **loses focus** (or the user presses Enter), not on every key.

- On a checkbox, `@bind` uses `checked` and a `bool`. On a `<select>`, it binds the selected `value`.
- For an `int` or `decimal` field, Blazor converts the text. If the text can't be parsed, the input goes back to the last valid value.

### `@bind:event`, `@bind:format`

```razor
<input @bind="search" @bind:event="oninput" />
<input type="text" @bind="startDate" @bind:format="dd/MM/yyyy" />
```

- **`@bind:event="oninput"`** updates the field on **every key**. Use it for live search or a character counter.
- **`@bind:format`** sets how a date is shown and parsed (for `DateTime`, `DateOnly` and similar types). With `type="date"`, Blazor already uses the right format, so don't add one.

### Run code after the value changes: `@bind:after`

```razor
<input @bind="search" @bind:event="oninput" @bind:after="FilterAsync" />
```

`@bind:after` runs a method (sync or `async Task`) **after** the field is updated. A common AI mistake is to write `@bind="search"` **and** `@onchange="Filter"` on the same element: that is a **build error**, because `@bind` already uses `onchange`. `@bind:after` is the fix.

### Full control: `@bind:get` / `@bind:set`

```razor
<input @bind:get="name" @bind:set="SetName" />

@code {
    private string name = "";
    private void SetName(string value) => name = value.Trim();
}
```

`@bind:get` gives the value to show and `@bind:set` receives the new value (an `Action<T>` or a `Func<T, Task>`). They are always used **together**. Use them when the new value needs to be changed or checked before it is stored.

## Two-way binding to your own component

A component can't change its own parameter (Level 2). Instead, it **asks the parent** to change it, with an **`EventCallback<T>`**. The convention is a pair: **`Value`** and **`ValueChanged`**.

```razor
@* Stepper.razor *@
<button @onclick="() => Change(-1)">-</button>
<span>@Value</span>
<button @onclick="() => Change(1)">+</button>

@code {
    [Parameter] public int Value { get; set; }
    [Parameter] public EventCallback<int> ValueChanged { get; set; }

    private async Task Change(int delta) => await ValueChanged.InvokeAsync(Value + delta);
}
```

```razor
<Stepper @bind-Value="quantity" />
<p>Quantity: @quantity</p>
```

- `@bind-Value="quantity"` is short for `Value="quantity"` plus `ValueChanged="v => quantity = v"`.
- The names must match: `@bind-X` needs a parameter `X` and an `EventCallback<T>` called **`XChanged`**.
- The child calls **`ValueChanged.InvokeAsync(newValue)`**. The parent stores the value, re-renders, and passes the new `Value` back down.

## Event handlers

```razor
<button @onclick="Increment">+1</button>
<button @onclick="() => Add(5)">+5</button>
<button @onclick="SaveAsync">Save</button>

@code {
    private int count;
    private void Increment() => count++;
    private void Add(int n) => count += n;
    private async Task SaveAsync() => await api.SaveAsync(count);
}
```

- Pass a **method group** (`Increment`, no parentheses) or a **lambda** to send arguments. `@onclick="Increment()"` is a mistake: it *calls* the method while rendering, and for a `void` method it doesn't even build.
- A handler can be `async Task`. Blazor re-renders when the handler **first awaits**, and **again when it finishes**. That is how a "Saving..." message can appear and then disappear. Avoid `async void`: Blazor can't wait for it or catch its errors.
- In a `@for` loop, copy the loop variable into a local first (`var index = i;`) and use the local in the lambda. Otherwise every button sees the final value of `i`. (`@foreach` doesn't have this problem.)

### Event arguments

The handler can take the event's data as a parameter:

```razor
<div @onmousemove="OnMove">Move here</div>
<input @onkeydown="OnKey" />
<select @onchange="OnPick">...</select>

@code {
    private void OnMove(MouseEventArgs e) => pos = $"{e.ClientX}, {e.ClientY}";
    private void OnKey(KeyboardEventArgs e) { if (e.Key == "Enter") Send(); }
    private void OnPick(ChangeEventArgs e) => choice = e.Value?.ToString();
}
```

- **`MouseEventArgs`**: `ClientX`, `ClientY`, `Button`, `ShiftKey`, `CtrlKey`.
- **`KeyboardEventArgs`**: `Key` (like `"Enter"` or `"a"`), `Code`, `CtrlKey`, `ShiftKey`.
- **`ChangeEventArgs`**: `Value` is an **`object?`**, so convert it (`e.Value?.ToString()`).

### `:preventDefault` and `:stopPropagation`

```razor
<a href="/next" @onclick="Track" @onclick:preventDefault>Next</a>

<div @onclick="CloseMenu">
    <button @onclick="Open" @onclick:stopPropagation>Open</button>
</div>
```

- **`@onclick:preventDefault`** stops the browser's default action (here, following the link). Your handler still runs.
- **`@onclick:stopPropagation`** stops the event from reaching parent elements, so clicking **Open** doesn't also run `CloseMenu`.
- Both can take a `bool`: `@onclick:preventDefault="shouldPrevent"`.

## `EventCallback` vs `Action`

```razor
[Parameter] public EventCallback OnSaved { get; set; }   // ✅
[Parameter] public Action? OnSaved { get; set; }         // ⚠️
```

Both let the child call the parent. The difference is **re-rendering**:

- When a child invokes an **`EventCallback`**, Blazor **re-renders the parent** (the component that owns the handler) automatically.
- With a plain **`Action`** or **`Func`**, the parent's method runs, but the parent is **not** re-rendered. Its UI stays old until something else renders it. AI code that uses `Action` parameters often has this "the screen doesn't update" bug.
- `EventCallback` also supports async handlers and is a `struct`, so it's never `null`. Calling an unset one does nothing.

## When does a component re-render?

1. **After an event handler** on that component (and again after an `async` one completes).
2. **When its parameters are set**, because its parent re-rendered.
3. **When you call `StateHasChanged()`**.

You only need `StateHasChanged()` when the state changes **outside** an event: a timer, a service event, a background task. From another thread, call `await InvokeAsync(StateHasChanged);`. Calling it at the end of a normal `@onclick` handler is useless: Blazor does it for you. You'll see this a lot in AI-generated code.

To skip renders, override **`ShouldRender()`** and return `false`. It is not used for the first render, and it's rarely needed.

## Cheat sheet

| You see | It means |
| --- | --- |
| `value="@x"` | Show only. Typing doesn't change `x`. |
| `@bind="x"` | Two-way, updated on `onchange` (blur or Enter). |
| `@bind:event="oninput"` | Updated on every key. |
| `@bind:after="M"` | Run `M` after `x` is updated. |
| `@bind:get` + `@bind:set` | Show a value, decide yourself how to store it. |
| `@bind-Value="x"` | Component with `Value` + `EventCallback<T> ValueChanged`. |
| `@onclick="M"` / `"() => M(1)"` | Method group / lambda with arguments. |
| `:preventDefault` / `:stopPropagation` | Block the browser action / stop the bubbling. |
| `EventCallback` | Parent re-renders. `Action` doesn't. |

## Checklist before you move on

- [ ] I can explain the difference between `value="@x"`, `@bind` and `@bind:event="oninput"`.
- [ ] I know why `@bind` plus `@onchange` doesn't build, and how `@bind:after` fixes it.
- [ ] I can write a component that supports `@bind-Value`.
- [ ] I can write handlers with a lambda, with event args, and with `async Task`.
- [ ] I can say when Blazor re-renders, and when `StateHasChanged()` is really needed.

Now close this tab and go to **Predict**.
