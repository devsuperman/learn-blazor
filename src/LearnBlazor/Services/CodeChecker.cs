using System.Text.RegularExpressions;
using LearnBlazor.Models;

namespace LearnBlazor.Services;

/// <summary>Runs an exercise's rules against the user's code.
/// This is pattern matching, not a compiler: it checks that the concept is used, not that the code builds.</summary>
public static class CodeChecker
{
    public sealed record Result(CodeCheck Check, bool Passed);

    public static IReadOnlyList<Result> Run(PracticeExercise exercise, string code) =>
        exercise.Checks
            .Select(c => new Result(c, IsMatch(c.Pattern, code) != c.MustNotMatch))
            .ToList();

    private static bool IsMatch(string pattern, string code)
    {
        try
        {
            return Regex.IsMatch(code, pattern, RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
