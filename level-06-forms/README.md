# Level 6: Forms and validation

Study this level on the site: **https://devsuperman.github.io/learn-blazor/topic/level-06-forms**.
Everything happens in the browser. You don't need a local project.

Content: [`src/LearnBlazor/wwwroot/content/level-06-forms/`](../src/LearnBlazor/wwwroot/content/level-06-forms/) (`theory.md` and `lesson.json`).

## Steps

1. **Theory:** `EditForm` with `Model` or `EditContext`, the submit callbacks (`OnValidSubmit`, `OnInvalidSubmit`, `OnSubmit`), `DataAnnotationsValidator`, `ValidationSummary` and `ValidationMessage`, data annotations, the built-in input components and `@bind-Value`, the CSS classes, forms under static SSR (`FormName`, `[SupplyParameterFromForm]`, `Enhance`), and the antiforgery token.
2. **Predict (4 questions):** a form with no validator, `OnSubmit` together with `OnValidSubmit`, a static SSR form with no name, and the CSS classes of a bad field.
3. **Quiz (10 questions):** `Model` and `EditContext` together, `ValidationMessage For`, `[Required]` on an `int`, the input for a `bool`, `FormName`, `[SupplyParameterFromForm]`, `Enhance`, `<AntiforgeryToken />`, `OnInvalidSubmit`, and `[Compare]`.
4. **Practice (6 exercises, written in a textbox):**
   1. Turn on validation (`DataAnnotationsValidator`, `ValidationSummary`, `ValidationMessage`)
   2. Write the rules (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]`)
   3. Fix the broken form (`OnSubmit`, `@bind-Value`, `type="submit"`)
   4. Pick the right inputs (`InputTextArea`, `InputDate`, `InputCheckbox`, `InputSelect`)
   5. Make it work under static SSR (`FormName`, `[SupplyParameterFromForm]`, `??=`, `Enhance`)
   6. A sign-up form, from scratch (`[Compare]`, password inputs)
5. **Interview (8 questions):** answer aloud first, then in writing.

## Pass

- The quiz is fully answered with at least 80% correct.
- All 6 exercises pass every check.

## Method check

- [ ] I passed every exercise without looking at the solution first.
- [ ] I scored at least 80% on my first quiz attempt.
- [ ] In an AI-generated form, I can find the model, its rules and the submit handler, and spot a missing validator or `FormName`.
- [ ] I answered at least 6 of 8 interview questions in under 30 seconds.
