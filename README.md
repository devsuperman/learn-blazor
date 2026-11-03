# learn-blazor

A playground to learn Blazor (.NET 10), one level at a time.

The goal: read AI-generated Blazor code with confidence and answer basic interview questions.
The full method lives in the study plan: [Blazor 10 Study Method: Prototype (Level 1)](https://claude.ai/artifact/XZTfZpeBAWV74AXrmBiFP8).

## Study site

Study in the browser at **https://devsuperman.github.io/learn-blazor/**.

Pick a level in the menu. Each level has one tab per step of the study loop: Theory, Predict, Quiz, Practice and Interview.
You write the practice code in a textbox on the site, so no local project is needed. Your answers and code are saved in your browser.

The site is a standalone Blazor WebAssembly app in [`src/LearnBlazor/`](src/LearnBlazor/). It deploys to GitHub Pages on every push to `main` (see [`.github/workflows/deploy-pages.yml`](.github/workflows/deploy-pages.yml)).

### Add a level to the site

1. Create `src/LearnBlazor/wwwroot/content/<level-id>/` with:
   - `theory.md`: the reading, in Markdown.
   - `lesson.json`: predict questions, practice exercises, quiz and interview questions. Copy the Level 1 file and follow its shape:
     - `predict` and `quiz` items are multiple choice: `body`, `options`, `correct` (0-based index) and `explanation`.
     - `practice` items have a `task`, `starter` and `solution` code, and `checks`. Each check is a .NET regex (`pattern`) that must match, or must not match when `mustNotMatch` is `true`.
2. In `src/LearnBlazor/wwwroot/content/topics.json`, set `"available": true` for that level.

## Status

| Level | Topic | Folder | Status |
| --- | --- | --- | --- |
| 1 | Render modes | [`level-01-render-modes/`](level-01-render-modes/) | Not started |
| 2 | Components and parameters | [`level-02-components/`](level-02-components/) | Not started |
| 3-10 | On hold until Level 2 is passed | - | Blocked |

## The study loop

1. **Theory:** read about one concept only.
2. **Predict:** read short snippets and pick what each one does. Your first pick counts.
3. **Quiz:** multiple-choice questions, without the theory open.
4. **Practice:** write code in the textbox and press **Check**. Type it yourself; the solution unlocks after your first check.
5. **Interview:** answer each question aloud (under 30 s), then in writing.

A level is **passed** when quiz is fully answered with at least 80% correct and every practice exercise passes all its checks.

Rules:

- Advance only after passing the level. If the quiz is below 80%, re-read the theory and retake it before you start the practice.
- From Level 3 on, start each session by redoing a practice exercise from 2 levels back.
- Code copied from AI does not count as a pass.

## Repo layout

```
src/LearnBlazor/                    # the study site (Blazor WebAssembly)
  wwwroot/content/<level-id>/       # theory.md + lesson.json for each level
level-XX-<topic>/README.md          # overview of each level
_template/                          # copy this to start a new level overview
```

## Requirements

Only to work on the site itself:

- .NET 10 SDK (`dotnet --version` should print 10.x)
