# Level 1: Render modes

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-01-render-modes**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-01-render-modes/`](../src/LearnBlazor/wwwroot/content/level-01-render-modes/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** static SSR, Interactive Server, Interactive WebAssembly, Interactive Auto, and prerendering.
2. **Predict (4 questions):** pick what each snippet does: no render mode, `InteractiveServer`, the double `OnInitialized`, and the WebAssembly console.
3. **Practice (5 exercises, written in a textbox):**
   1. A static page with `RendererInfo`
   2. Make a counter interactive on the server
   3. Run it in the browser (WebAssembly)
   4. Stop the double run (`prerender: false`)
   5. The `/modes` page, from scratch
4. **Quiz (10 questions):** what you would see in dev tools, `RendererInfo` values, the `.Client` project, Auto mode, and the global render mode.
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- All 5 exercises pass every check.
- The quiz is fully answered with at least 80% correct.

## Method check (the method works if 3 of 4 are true)

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] I can name the render mode in an AI-generated file and explain why it was chosen.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
