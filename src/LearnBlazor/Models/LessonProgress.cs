namespace LearnBlazor.Models;

public enum TopicStatus { NotStarted, InProgress, Passed }

/// <summary>Everything the user did on one level. Stored as JSON in localStorage.</summary>
public sealed class LessonProgress
{
    /// <summary>Predict question id → chosen option index. Locked once chosen.</summary>
    public Dictionary<string, int> PredictChoices { get; set; } = [];
    /// <summary>Exercise id → the code in the textbox.</summary>
    public Dictionary<string, string> PracticeCode { get; set; } = [];
    /// <summary>Exercises checked at least once (unlocks the solution).</summary>
    public HashSet<string> PracticeChecked { get; set; } = [];
    /// <summary>Exercises whose last check passed every rule.</summary>
    public HashSet<string> PracticePassed { get; set; } = [];
    /// <summary>Quiz question id → chosen option index. Locked until the quiz is retaken.</summary>
    public Dictionary<string, int> QuizChoices { get; set; } = [];
    public Dictionary<string, string> InterviewAnswers { get; set; } = [];
    public HashSet<string> InterviewRevealed { get; set; } = [];
    /// <summary>Kept in sync with <see cref="PassRule"/> on every save, so menus can show it without the lesson.</summary>
    public bool Passed { get; set; }

    public TopicStatus Status =>
        Passed ? TopicStatus.Passed
        : HasAnyActivity ? TopicStatus.InProgress
        : TopicStatus.NotStarted;

    private bool HasAnyActivity =>
        PredictChoices.Count > 0
        || PracticeChecked.Count > 0
        || QuizChoices.Count > 0
        || InterviewAnswers.Values.Any(v => !string.IsNullOrWhiteSpace(v));
}

/// <summary>A level is passed when every practice exercise passes its checks
/// and the whole quiz is answered with at least <see cref="Lesson.QuizPassPercent"/> correct.</summary>
public static class PassRule
{
    public static int QuizCorrect(Lesson lesson, LessonProgress p) =>
        lesson.Quiz.Count(q => p.QuizChoices.TryGetValue(q.Id, out var c) && c == q.Correct);

    public static bool QuizComplete(Lesson lesson, LessonProgress p) =>
        lesson.Quiz.All(q => p.QuizChoices.ContainsKey(q.Id));

    public static int QuizPercent(Lesson lesson, LessonProgress p) =>
        lesson.Quiz.Count == 0 ? 100 : QuizCorrect(lesson, p) * 100 / lesson.Quiz.Count;

    public static bool QuizPassed(Lesson lesson, LessonProgress p) =>
        QuizComplete(lesson, p) && QuizPercent(lesson, p) >= lesson.QuizPassPercent;

    public static int PracticePassedCount(Lesson lesson, LessonProgress p) =>
        lesson.Practice.Count(e => p.PracticePassed.Contains(e.Id));

    public static bool PracticeDone(Lesson lesson, LessonProgress p) =>
        PracticePassedCount(lesson, p) == lesson.Practice.Count;

    public static bool IsPassed(Lesson lesson, LessonProgress p) =>
        PracticeDone(lesson, p) && QuizPassed(lesson, p);
}
