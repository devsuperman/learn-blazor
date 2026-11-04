# Level 10: Calling APIs and auth

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-10-data-auth**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-10-data-auth/`](../src/LearnBlazor/wwwroot/content/level-10-data-auth/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** registering `HttpClient` (WebAssembly `BaseAddress`, `IHttpClientFactory` and typed clients on the server), `GetFromJsonAsync` and `PostAsJsonAsync`, loading/error/null states, why WebAssembly needs a web API (and CORS), one service with two implementations for Auto components, authentication vs authorization, `AuthenticationStateProvider` and the cascading `Task<AuthenticationState>`, `AuthorizeView`, `[Authorize]` and `AuthorizeRouteView`, and why the API must enforce authorization.
2. **Predict (4 questions):** a list with no null check, a POST that fails without an exception, `[Authorize]` on a child component, and `AuthorizeView` with the wrong role.
3. **Quiz (10 questions):** WebAssembly and databases, `GetFromJsonAsync` on 404, `PostAsJsonAsync` on 500, CORS, `Authorizing`, `[Authorize]` without `@page`, `Roles` with a comma, reading the user in code, `RouteView` vs `AuthorizeRouteView`, and hidden UI vs API security.
4. **Practice (6 exercises, written in a textbox):**
   1. Register HttpClient in WebAssembly (`BaseAddress`)
   2. Loading, error, and data (`try/catch`, `HttpRequestException`)
   3. Check the POST response (`IsSuccessStatusCode`)
   4. Show and hide with AuthorizeView (`Authorized`, `NotAuthorized`, `Roles`)
   5. Protect the page and the API (`[Authorize]`, `RequireAuthorization`)
   6. Read the user in code, from scratch (cascading `Task<AuthenticationState>`, `IsInRole`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In AI-generated code, I can find where the `HttpClient` comes from and what happens when a call fails.
- [ ] In AI-generated code, I can tell whether the API, not only the UI, checks authorization.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
