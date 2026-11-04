# Level 4: Component lifecycle

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-04-lifecycle**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-04-lifecycle/`](../src/LearnBlazor/wwwroot/content/level-04-lifecycle/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** the order of calls, `SetParametersAsync`, `OnInitialized{Async}` (and why it runs twice with prerendering), the render before an `await` finishes, `OnParametersSet{Async}`, `OnAfterRender{Async}(firstRender)` and JS interop, `StateHasChanged` and `ShouldRender`, and `IDisposable` / `IAsyncDisposable`.
2. **Predict (4 questions):** the order of calls, a page with no loading state, `OnInitialized` with prerendering, and a timer that doesn't re-render.
3. **Quiz (10 questions):** `OnParametersSet`, where JS interop goes, the render during an `await`, loading twice with prerendering, `IDisposable` plus `IAsyncDisposable`, a render loop, `ShouldRender`, `InvokeAsync(StateHasChanged)`, the order of calls, and `OnAfterRender` in static SSR.
4. **Practice (6 exercises, written in a textbox):**
   1. Add a loading state (`OnInitializedAsync`, null check)
   2. Reload when the parameter changes (`OnParametersSetAsync`)
   3. JavaScript after the first render (`OnAfterRenderAsync`, `firstRender`, `StateHasChanged`)
   4. Unsubscribe when the component is removed (`@implements IDisposable`)
   5. A clock that ticks and stops (timer, `InvokeAsync(StateHasChanged)`, dispose)
   6. A cancellable load, from scratch (`CancellationTokenSource`, `Dispose`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In an AI-generated page, I can say when its data loads, how often, and what shows before it arrives.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
