# learn-blazor

A playground to learn Blazor (.NET 10), one level at a time.

The goal: read AI-generated Blazor code with confidence and answer basic interview questions.
The full method lives in the study plan: [Blazor 10 Study Method: Prototype (Level 1)](https://claude.ai/artifact/XZTfZpeBAWV74AXrmBiFP8).

## Study site

Study in the browser at **https://devsuperman.github.io/learn-blazor/**.

Pick a level in the menu. Each level has one tab per step of the study loop: Theory, Predict, Quiz, Practice and Interview.
You write the practice code in a textbox on the site, so no local project is needed. Your answers and code are saved in your browser.

The site is a standalone Blazor WebAssembly app in [`src/LearnBlazor/`](src/LearnBlazor/). It deploys to GitHub Pages on every push to `main` (see [`.github/workflows/deploy-pages.yml`](.github/workflows/deploy-pages.yml)).

### Run the site locally

Needs the .NET 10 SDK (`dotnet --version` should print 10.x).

```bash
dotnet run --project src/LearnBlazor
```

Then open http://localhost:5171. Content files in `wwwroot/content/` are fetched without the browser cache, so a refresh shows your edits.

### Add a level to the site

1. Create `src/LearnBlazor/wwwroot/content/<level-id>/` with:
   - `theory.md`: the reading, in Markdown.
   - `lesson.json`: predict questions, practice exercises, quiz and interview questions. Copy an existing file and follow its shape (the C# model is [`Models/Lesson.cs`](src/LearnBlazor/Models/Lesson.cs)):
     - Top level: `id` (same as the folder), `number`, `title`, `summary`, and optional `quizPassPercent` (default 80).
     - `predict` and `quiz` items are multiple choice: `id`, `body`, `options`, `correct` (0-based index) and `explanation`. Predict items also have a short `title`.
     - `practice` items have an `id`, `title`, `task`, `starter` and `solution` code, an optional `hint`, and `checks`. Each check has an `id`, a `label`, a .NET regex (`pattern`) that must match (or must not match when `mustNotMatch` is `true`), and an optional `hint` shown when it fails.
     - `interview` items have an `id`, a `question` and a `modelAnswer`.
     - All text fields except `pattern` are Markdown.
2. In `src/LearnBlazor/wwwroot/content/topics.json`, set `"available": true` for that level.
3. Copy [`_template/`](_template/) to `level-XX-<topic>/` and fill in the overview.

Progress is saved per level and per item `id`, so changing an `id` after release resets that item for everyone who answered it.

## Levels

All 10 levels are available on the site. Your progress (Not started, In progress, Passed) is shown in the site menu; it is stored in your browser, not in this repo.

| Level | Topic | Site | Overview |
| --- | --- | --- | --- |
| 1 | Render modes | [Study](https://devsuperman.github.io/learn-blazor/topic/level-01-render-modes) | [`level-01-render-modes/`](level-01-render-modes/) |
| 2 | Components and parameters | [Study](https://devsuperman.github.io/learn-blazor/topic/level-02-components) | [`level-02-components/`](level-02-components/) |
| 3 | Data binding and events | [Study](https://devsuperman.github.io/learn-blazor/topic/level-03-binding-events) | [`level-03-binding-events/`](level-03-binding-events/) |
| 4 | Component lifecycle | [Study](https://devsuperman.github.io/learn-blazor/topic/level-04-lifecycle) | [`level-04-lifecycle/`](level-04-lifecycle/) |
| 5 | Routing and navigation | [Study](https://devsuperman.github.io/learn-blazor/topic/level-05-routing) | [`level-05-routing/`](level-05-routing/) |
| 6 | Forms and validation | [Study](https://devsuperman.github.io/learn-blazor/topic/level-06-forms) | [`level-06-forms/`](level-06-forms/) |
| 7 | Dependency injection | [Study](https://devsuperman.github.io/learn-blazor/topic/level-07-dependency-injection) | [`level-07-dependency-injection/`](level-07-dependency-injection/) |
| 8 | State and cascading values | [Study](https://devsuperman.github.io/learn-blazor/topic/level-08-state) | [`level-08-state/`](level-08-state/) |
| 9 | JavaScript interop | [Study](https://devsuperman.github.io/learn-blazor/topic/level-09-js-interop) | [`level-09-js-interop/`](level-09-js-interop/) |
| 10 | Calling APIs and auth | [Study](https://devsuperman.github.io/learn-blazor/topic/level-10-data-auth) | [`level-10-data-auth/`](level-10-data-auth/) |

## The study loop

1. **Theory:** read about one concept only.
2. **Predict:** read short snippets and pick what each one does. Your first pick counts.
3. **Quiz:** multiple-choice questions, without the theory open.
4. **Practice:** write code in the textbox and press **Check**. Type it yourself; the solution unlocks after your first check.
5. **Interview:** answer each question aloud (under 30 s), then in writing.

A level is **passed** when the quiz is fully answered with at least 80% correct and every practice exercise passes all its checks.

Rules:

- Advance only after passing the level. If the quiz is below 80%, re-read the theory and retake it before you start the practice.
- From Level 3 on, start each session by redoing a practice exercise from 2 levels back.
- Code copied from AI does not count as a pass.

## Repo layout

```
src/LearnBlazor/                    # the study site (Blazor WebAssembly, .NET 10)
  Pages/                            # Home (overview), Topic (one level, one tab per step), NotFound
  Components/                       # the tabs (Predict, Quiz, Practice, Interview) and shared UI
  Services/                         # ContentService (loads content), ProgressService (localStorage), CodeChecker (regex checks)
  Models/                           # Lesson (lesson.json shape), LessonProgress and PassRule
  wwwroot/content/topics.json       # the level list shown in the menu
  wwwroot/content/<level-id>/       # theory.md + lesson.json for each level
level-XX-<topic>/README.md          # overview of each level
_template/                          # copy this to start a new level overview
.github/workflows/deploy-pages.yml  # publishes the site to GitHub Pages
CLAUDE.md                           # notes for Claude Code
```

Practice checks are pattern matching, not a compiler: they check that a concept is used, not that the code builds.
