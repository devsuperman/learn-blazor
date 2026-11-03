using System.Text;
using LearnBlazor.Models;

namespace LearnBlazor.Services;

/// <summary>Builds a Markdown file with everything the user wrote for one level.</summary>
public static class NotesExport
{
    public static string ToMarkdown(Lesson lesson, LessonProgress p)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Level {lesson.Number} notes: {lesson.Title}");
        sb.AppendLine();
        sb.AppendLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm}  ");
        sb.AppendLine($"Status: {p.Status}  ");
        sb.AppendLine($"Challenge attempts: {p.ChallengeAttempts}");
        sb.AppendLine();

        sb.AppendLine("## Predictions");
        foreach (var item in lesson.Predict)
            sb.AppendLine($"- **{item.Id} · {item.Title}:** {Value(p.Predictions, item.Id)}");
        sb.AppendLine();

        sb.AppendLine("## Practice");
        foreach (var step in lesson.Practice)
        {
            var firstLine = step.Text.Split('\n')[0];
            sb.AppendLine($"- [{(p.PracticeDone.Contains(step.Id) ? "x" : " ")}] {firstLine}");
        }
        sb.AppendLine();

        sb.AppendLine("## Challenge answers");
        foreach (var q in lesson.Challenge.Questions)
            sb.AppendLine($"1. {q.Text}\n   {Value(p.ChallengeAnswers, q.Id)}");
        sb.AppendLine();

        sb.AppendLine("## 3-line note");
        sb.AppendLine($"- What it is: {p.NoteWhat}");
        sb.AppendLine($"- How it differs from what I know: {p.NoteDiffers}");
        sb.AppendLine($"- One gotcha: {p.NoteGotcha}");
        sb.AppendLine();

        sb.AppendLine("## Interview answers");
        foreach (var q in lesson.Interview)
            sb.AppendLine($"1. {q.Question}\n   {Value(p.InterviewAnswers, q.Id)}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(p.FreeNotes))
        {
            sb.AppendLine("## Free notes");
            sb.AppendLine(p.FreeNotes);
        }

        return sb.ToString();
    }

    private static string Value(Dictionary<string, string> d, string key) =>
        d.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ReplaceLineEndings(" ")
            : "_(empty)_";
}
