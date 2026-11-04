# Level 7: Dependency injection

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-07-dependency-injection**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-07-dependency-injection/`](../src/LearnBlazor/wwwroot/content/level-07-dependency-injection/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** registering services in `Program.cs` (`AddSingleton`, `AddScoped`, `AddTransient`, interface and implementation), `@inject`, `[Inject]` and constructor injection, what Scoped means in static SSR, Interactive Server and WebAssembly, why a server Singleton is shared by all users, transient `IDisposable` services, `OwningComponentBase`, `IDbContextFactory` in Blazor Server, registering in both projects for Auto, and built-in services.
2. **Predict (4 questions):** a singleton counter shared by two users, Scoped across navigation and a new tab, the missing client registration in Auto mode, and two transients.
3. **Quiz (10 questions):** Scoped per circuit, Scoped in WebAssembly, Scoped in static SSR, `@inject` syntax, the Singleton data leak, `IDbContextFactory`, transient disposables, `OwningComponentBase<T>`, the missing `.Client` registration, and `HttpClient` on the server.
4. **Practice (6 exercises, written in a textbox):**
   1. Register a service (`AddScoped<ICartService, CartService>`)
   2. Inject it in a page (`@inject`, `NavigationManager`)
   3. Inject in code-behind (`[Inject]`, `ILogger<T>`)
   4. Fix the data leak (Singleton to Scoped)
   5. Use a DbContext factory (`IDbContextFactory<T>`)
   6. Register for Auto, from scratch (server and `.Client` `Program.cs`)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In AI-generated code, I can spot a Singleton that holds per-user data, and a `DbContext` injected into an interactive server component.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
