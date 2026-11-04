# Level 8: State and cascading values

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-08-state**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-08-state/`](../src/LearnBlazor/wwwroot/content/level-08-state/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** `<CascadingValue>` and `[CascadingParameter]`, `Name` and `IsFixed`, root-level cascading values (`AddCascadingValue`), the render mode boundary, a scoped state container with `OnChange`, where state lives and is lost, protected browser storage, and persisting prerendered state (`[PersistentState]` and `PersistentComponentState`).
2. **Predict (4 questions):** a cascading parameter of the wrong type, a cascading value that doesn't reach an interactive island, a count that doesn't update, and how many times a prerendered page fetches.
3. **Quiz (10 questions):** matching by type, `IsFixed`, root-level values, `RenderFragment` across a render mode boundary, the state container's lifetime, `InvokeAsync(StateHasChanged)`, a refresh on Interactive Server, protected storage during prerendering, `TryTakeFromJson`, and `[PersistentState]`.
4. **Practice (6 exercises, written in a textbox):**
   1. Read a cascading value (`[CascadingParameter]`)
   2. Provide a fixed value (`<CascadingValue>`, `IsFixed`)
   3. A state container (`event Action? OnChange`, `NotifyStateChanged`)
   4. Subscribe and unsubscribe (`IDisposable`, `InvokeAsync(StateHasChanged)`)
   5. Stop the double fetch with `[PersistentState]`
   6. Persist state by hand, from scratch (`PersistentComponentState`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In AI-generated code, I can say where each piece of state lives and when it is lost.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
