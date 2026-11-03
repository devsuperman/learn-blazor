# learn-blazor

A playground to learn Blazor (.NET 10), one level at a time.

The goal: read AI-generated Blazor code with confidence and answer basic interview questions.
The full method lives in the study plan: [Blazor 10 Study Method: Prototype (Level 1)](https://claude.ai/artifact/XZTfZpeBAWV74AXrmBiFP8).

## Study site

Study in the browser at **https://devsuperman.github.io/learn-blazor/**.

Pick a level in the menu. Each level has one tab per step of the study loop: Theory, Predict, Practice, Challenge, Note and Interview. The site includes timers, and your answers are saved in your browser (you can download them as Markdown).
The exercises themselves (practice and challenge code) are yours to write and run on your machine.

The site is a standalone Blazor WebAssembly app in [`src/LearnBlazor/`](src/LearnBlazor/). It deploys to GitHub Pages on every push to `main` (see [`.github/workflows/deploy-pages.yml`](.github/workflows/deploy-pages.yml)).

### Add a level to the site

1. Create `src/LearnBlazor/wwwroot/content/<level-id>/` with:
   - `theory.md`: the reading, in Markdown.
   - `lesson.json`: predict snippets, practice steps, challenge, rubric and interview questions. Copy the Level 1 file and follow its shape.
2. In `src/LearnBlazor/wwwroot/content/topics.json`, set `"available": true` for that level.

## Status

| Level | Topic | Folder | Status |
| --- | --- | --- | --- |
| 1 | Render modes | [`level-01-render-modes/`](level-01-render-modes/) | Not started |
| 2-10 | On hold until Level 1 validates the method | - | Blocked |

## The study loop (60-90 min per level)

1. **Theory (15 min):** read about one concept only, then close the tab.
2. **Predict (5 min):** read a short example and write what it will do, before running it.
3. **Practice (30 min):** write code that uses the concept. No copy-paste; docs are allowed.
4. **Challenge (15 min):** solve a small problem from scratch, without the theory open.
5. **Note (5 min):** 3 lines: what it is, how it differs from what you know, one gotcha.

Rules:

- Advance only after passing the challenge without help. Otherwise repeat the level with a new exercise.
- The timer ends each block, not your mood.
- From Level 3 on, start each session with a 5-minute review challenge from 2 levels back.
- Code copied from AI does not count as a Pass.

## Repo layout

```
level-XX-<topic>/
  README.md   # the lab steps for this level
  notes.md    # predictions, challenge answers, 3-line note, interview answers
  code/       # your own code: practice app + challenge
_template/    # copy this to start a new level
```

Start a new level:

```bash
cp -r _template level-02-components
```

## Requirements

- .NET 10 SDK (`dotnet --version` should print 10.x)
- Any IDE (Visual Studio, VS Code with C# Dev Kit, or Rider)
