# Level 9: JavaScript interop

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-09-js-interop**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-09-js-interop/`](../src/LearnBlazor/wwwroot/content/level-09-js-interop/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** injecting `IJSRuntime`, `InvokeVoidAsync` vs `InvokeAsync<T>`, identifiers as paths on `window`, why JS calls fail during prerendering (and why they belong in `OnAfterRenderAsync`), JS modules and collocated `.razor.js` files, disposing modules and `JSDisconnectedException`, `ElementReference` with `@ref`, calling .NET from JS (`[JSInvokable]`, `DotNet.invokeMethodAsync`, `DotNetObjectReference`), `IJSInProcessRuntime`, and the new .NET 10 methods.
2. **Predict (4 questions):** a JS call in `OnInitializedAsync` with prerendering, a call from JS without `StateHasChanged`, the wrong assembly name in `DotNet.invokeMethodAsync`, and a module export called through `window`.
3. **Quiz (10 questions):** `InvokeAsync<T>` vs `InvokeVoidAsync`, identifiers on `window`, where to call JS on load, `import` and `IJSObjectReference`, collocated module paths, `JSDisconnectedException`, when `@ref` is set, static `[JSInvokable]` methods, disposing `DotNetObjectReference`, and `IJSInProcessRuntime`.
4. **Practice (6 exercises, written in a textbox):**
   1. Save the theme (`InvokeVoidAsync`, `localStorage.setItem`)
   2. Fix the prerender crash (`OnAfterRenderAsync`, `firstRender`)
   3. Load a collocated module (`import`, `IJSObjectReference`)
   4. Dispose the module safely (`IAsyncDisposable`, `JSDisconnectedException`)
   5. Focus the input (`@ref`, `ElementReference`)
   6. Let JS call your component, from scratch (`DotNetObjectReference`, `[JSInvokable]`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In AI-generated code, I can spot a JS call that runs too early and a JS or .NET reference that is never disposed.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
