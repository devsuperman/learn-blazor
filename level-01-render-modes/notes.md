# Level 1 notes: Render modes

Date: 
Time spent: theory __ / predict __ / practice __ / challenge __ / note __ (total __ min)
Challenge attempts: __

## Predictions (write before running)

| Snippet | My prediction | What actually happened |
| --- | --- | --- |
| A: no `@rendermode`, click the button | | |
| B: `InteractiveServer`, click the button | | |
| C: how many times does "C initialized" print? | | |

## Practice observations

| Page | Count changes? | `RendererInfo.Name` | `IsInteractive` |
| --- | --- | --- | --- |
| `/static` | | | |
| `/server` | | | |
| `/wasm` | | | |
| `/auto` | | | |

- Console lines on `/server` with prerendering: __
- Console lines on `/server` without prerendering: __

## Challenge answers (own words, no AI)

1. Why does the button work on `/server` but not on `/static`?

2. On `/wasm`, where does the C# code run, and what does the browser download first?

3. Why did the console line print twice before you disabled prerendering?

## 3-line note

- What it is:
- How it differs from what I know (Razor Pages / MVC):
- One gotcha:

## Interview questions

1. What are the Blazor render modes?
2. When would you choose Server over WebAssembly?
3. When would you choose WebAssembly over Server?
4. What does Auto mode do?
5. What is prerendering and what problem can it cause?
6. How does the UI update in Interactive Server?
7. Why does a button do nothing on a static page?
8. How do you tell which mode a component is running in?

## Review result

- Result: Pass / Needs work
- What failed:
- Follow-up question and my answer:
