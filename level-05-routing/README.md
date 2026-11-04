# Level 5: Routing and navigation

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-05-routing**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-05-routing/`](../src/LearnBlazor/wwwroot/content/level-05-routing/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** `@page` (several per component), the `Router` and the not-found page (`NotFoundPage`, `NavigationManager.NotFound()`), route parameters and constraints, optional and catch-all parameters, query strings with `[SupplyParameterFromQuery]`, `NavigationManager`, `NavLink` and `NavLinkMatch`, and why route-based data loads in `OnParametersSet{Async}`.
2. **Predict (4 questions):** a constraint that doesn't match, the same page with a new id, the Home link that is always active, and which query value a property reads.
3. **Quiz (10 questions):** several `@page` lines, constraint syntax, catch-all, `forceLoad`, `GetUriWithQueryParameter`, `NavLinkMatch.Prefix`, `ToBaseRelativePath`, optional parameters with a constraint, `replace`, and `NotFound()`.
4. **Practice (6 exercises, written in a textbox):**
   1. Two URLs for one page
   2. Read a route parameter (`{Id:int}`)
   3. An optional parameter (`{Status?}`)
   4. Fix the page that doesn't update (`OnParametersSetAsync`)
   5. Search with the query string (`[SupplyParameterFromQuery]`, `GetUriWithQueryParameter`)
   6. A nav menu, from scratch (`NavLink`, `NavLinkMatch.All`, `ActiveClass`, `forceLoad`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In AI-generated code, I can list a page's URLs and say where each route and query value goes.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
