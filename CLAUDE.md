# CLAUDE.md

Notes for Claude Code working in this repo.

## What this is

A study site for learning Blazor (.NET 10) in 10 levels. It is a standalone **Blazor WebAssembly** app in `src/LearnBlazor/`,
published to GitHub Pages at https://devsuperman.github.io/learn-blazor/. There is no backend: content is static files,
progress is stored in the browser's `localStorage`.

Most changes are **content** (theory, questions, exercises), not code.

## Commands

```bash
dotnet build src/LearnBlazor
dotnet run --project src/LearnBlazor      # http://localhost:5171
dotnet publish src/LearnBlazor/LearnBlazor.csproj -c Release -o publish   # what CI runs
```

There are no tests. Before committing content changes, at least check that every `lesson.json` is valid JSON
and every `pattern` is a valid regex (the C# side uses .NET regex with `RegexOptions.Multiline`).

## Layout

- `src/LearnBlazor/wwwroot/content/topics.json`: the level list (menu). `available: false` hides a level.
- `src/LearnBlazor/wwwroot/content/<level-id>/theory.md` and `lesson.json`: one level's content.
- `src/LearnBlazor/Models/Lesson.cs`: the `lesson.json` shape. `Models/LessonProgress.cs`: saved progress and `PassRule`.
- `src/LearnBlazor/Services/`: `ContentService` (fetches content, no browser cache), `ProgressService` (localStorage,
  key `learn-blazor:progress:<level-id>`), `CodeChecker` (runs practice regex checks, 1 s timeout).
- `src/LearnBlazor/Pages/Topic.razor`: one level, with tabs via `?tab=theory|predict|quiz|practice|interview`.
- `src/LearnBlazor/Components/`: one component per tab, plus shared UI.
- `level-XX-<topic>/README.md`: a short overview per level that mirrors its `lesson.json`. `_template/` is the starting point.
- `.github/workflows/deploy-pages.yml`: deploys on push to `main`; rewrites `<base href>` to `/learn-blazor/` and copies
  `index.html` to `404.html` for deep links.

## Content rules

- Each level has 4 predict, 10 quiz (multiple choice), 6 practice exercises (Level 1 has 5) and 8 interview questions.
- `correct` is a 0-based index into `options`. Spread correct answers across positions.
- Item `id`s are progress keys. Don't rename or reuse them in released levels; it resets or corrupts saved answers.
- Practice checks are regexes against the learner's code, not a compiler. Make them tolerant of whitespace and
  formatting (`\s*`, `\s+`), and make sure the `solution` passes every check and the `starter` does not.
- All text fields except `pattern` are Markdown. Code blocks use ```` ```razor ```` or ```` ```csharp ````.
- When you change a level's `lesson.json`, update its `level-XX-<topic>/README.md` (question counts, exercise titles,
  topics listed) so they stay in sync.
- Targets .NET 10 Blazor APIs (e.g. `RendererInfo`, `[PersistentState]`, `NavigationManager.NotFound()`).

## Writing style

Plain, short English. Short sentences, simple words, second person ("you"). Use the existing levels as the style reference.
