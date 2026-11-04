# Level 3: Data binding and events

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-03-binding-events**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-03-binding-events/`](../src/LearnBlazor/wwwroot/content/level-03-binding-events/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** events need an interactive render mode, one-way `value="@x"`, `@bind` and its default `onchange` event, `@bind:event="oninput"`, `@bind:format`, `@bind:after`, `@bind:get`/`@bind:set`, two-way binding to a child (`Value`/`ValueChanged` and `@bind-Value`), `@onclick` with lambdas and async handlers, event args, `:preventDefault` and `:stopPropagation`, `EventCallback` vs `Action`, and when a component re-renders (`StateHasChanged`, `ShouldRender`).
2. **Predict (4 questions):** a one-way value, the default bind event, `Action` instead of `EventCallback`, and an async handler.
3. **Quiz (10 questions):** the default bind event, calling a handler while rendering, `@bind` plus `@onchange`, `EventCallback` and re-rendering, `@bind-Color`, `ChangeEventArgs.Value`, when to call `StateHasChanged()`, `stopPropagation`, a button that does nothing, and async re-renders.
4. **Practice (6 exercises, written in a textbox):**
   1. Make it two-way (`@bind`, `@bind:event="oninput"`)
   2. Wire up the buttons (method group and lambda)
   3. Fix the double onchange (`@bind:after`)
   4. Press Enter to add (`KeyboardEventArgs`)
   5. Make the stepper bindable (`EventCallback<int> ValueChanged`)
   6. A bindable search box, from scratch (`@bind:get`, `@bind:set`, `TextChanged`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In an AI-generated component, I can say which event updates each bound value, and which component re-renders after it.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
