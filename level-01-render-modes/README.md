# Level 1: Render modes

About 60-90 minutes in total. Theory and the snippets are in the [study plan](https://claude.ai/artifact/XZTfZpeBAWV74AXrmBiFP8).
Fill in [`notes.md`](notes.md) as you go.

## Checklist

### Theory (15 min)

- [ ] Read the render modes section of the study plan (or the official .NET 10 Blazor docs), then close it.

### Predict (5 min)

- [ ] Write your predictions for snippets A, B and C in `notes.md` **before** running anything.

### Practice (30 min)

- [ ] Create the app in `code/`:

  ```bash
  cd code && dotnet new blazor -n RenderModesLab -int Auto
  ```

  If the options differ, check `dotnet new blazor --help`.
- [ ] Add four pages, each with a counter button and a line showing `RendererInfo.Name` and `RendererInfo.IsInteractive`:
  - `/static`: no render mode
  - `/server`: `InteractiveServer`
  - `/wasm`: `InteractiveWebAssembly` (in the `.Client` project)
  - `/auto`: `InteractiveAuto` (in the `.Client` project)
- [ ] Run the app. Record whether the count changes on each page.
- [ ] Dev tools: find the WebSocket on `/server`, the `.wasm` downloads on `/wasm`, and the complete HTML in View Source on `/static`.
- [ ] On `/server`, add `Console.WriteLine` in `OnInitialized`. Count the lines after one page load.
- [ ] Disable prerendering with `@rendermode @(new InteractiveServerRenderMode(prerender: false))`. Count again.

### Challenge (15 min, from scratch)

- [ ] Close the theory. Build a `/modes` page with `InteractiveServer` that shows the render mode name, whether it is interactive, a click counter, and the time of the last click.
- [ ] Answer the 3 challenge questions in `notes.md`, in your own words and without AI.

### Note (5 min)

- [ ] Write the 3-line note in `notes.md`.

### Review

- [ ] Paste your `/modes` code and the 3 answers into a chat, saying "Level 1". It must work locally with `dotnet run` first.
- [ ] Answer the follow-up question.
- [ ] Answer the 8 interview questions aloud (under 30 s each), then in `notes.md`.

## Rubric (Pass only if all 5 are met)

| # | Criterion | Pass looks like |
| --- | --- | --- |
| 1 | Render mode | `@rendermode InteractiveServer` is on the `/modes` page |
| 2 | Mode info | The page shows `RendererInfo.Name` and `RendererInfo.IsInteractive` |
| 3 | Counter | `@onclick` increments the count, with no needless `StateHasChanged` |
| 4 | Last click time | A field stores the time and the page displays it |
| 5 | Written answers | Q1: static = HTML only, no circuit. Q2: runs in the browser after runtime + assemblies download. Q3: prerender on the server, then again when interactive |

## Validation (method passes if 3 of 4 are true)

- [ ] The whole level fit in 60-90 minutes.
- [ ] I passed the challenge without help in at most 2 attempts.
- [ ] I can name the render mode in an AI-generated file and explain why it was chosen.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
