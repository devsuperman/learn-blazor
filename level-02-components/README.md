# Level 2: Components and parameters

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-02-components**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-02-components/`](../src/LearnBlazor/wwwroot/content/level-02-components/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** component anatomy, using components, `[Parameter]` and `[EditorRequired]`, not writing to your own parameters, `ChildContent` and named slots, `RenderFragment<T>` templates, attribute splatting, and code-behind.
2. **Predict (4 questions):** a property without `[Parameter]`, the toggle that resets, child content with the wrong name, and a template with `context`.
3. **Quiz (10 questions):** file name and class, `[EditorRequired]`, re-rendering, a missing `@using`, named slots, `context`, attribute splatting, code-behind, and string parameters.
4. **Practice (6 exercises, written in a textbox):**
   1. Your first component (`Badge`)
   2. Make it a real parameter (`[Parameter]`, `[EditorRequired]`, a default)
   3. Fix the toggle that resets
   4. A card with child content
   5. Fill the slots (`Header` and `Body`)
   6. A generic list, from scratch (`@typeparam`, `RenderFragment<TItem>`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In an AI-generated component, I can list its parameters and say how a parent would use it.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
