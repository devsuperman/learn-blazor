namespace LearnBlazor.Models;

/// <summary>One entry in the sidebar menu (wwwroot/content/topics.json).</summary>
public sealed record TopicSummary(
    string Id,
    int Number,
    string Title,
    string Summary,
    bool Available);

/// <summary>All study content for one level (wwwroot/content/{id}/lesson.json).
/// The theory text lives next to it in theory.md.</summary>
public sealed class Lesson
{
    public string Id { get; set; } = "";
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<PredictItem> Predict { get; set; } = [];
    public List<PracticeStep> Practice { get; set; } = [];
    public Challenge Challenge { get; set; } = new();
    public List<InterviewQuestion> Interview { get; set; } = [];
}

public sealed class PredictItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>Markdown with the snippet and the question.</summary>
    public string Body { get; set; } = "";
    /// <summary>Markdown shown after the user writes a prediction.</summary>
    public string Answer { get; set; } = "";
}

public sealed class PracticeStep
{
    public string Id { get; set; } = "";
    /// <summary>Markdown.</summary>
    public string Text { get; set; } = "";
    /// <summary>Optional markdown hint, hidden until requested.</summary>
    public string? Hint { get; set; }
}

public sealed class Challenge
{
    public int Minutes { get; set; } = 15;
    /// <summary>Markdown.</summary>
    public string Statement { get; set; } = "";
    public List<ChallengeQuestion> Questions { get; set; } = [];
    public List<RubricItem> Rubric { get; set; } = [];
}

public sealed class ChallengeQuestion
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    /// <summary>Markdown, shown only after the challenge is finished.</summary>
    public string ModelAnswer { get; set; } = "";
}

public sealed class RubricItem
{
    public string Id { get; set; } = "";
    public string Criterion { get; set; } = "";
    public string PassLooksLike { get; set; } = "";
}

public sealed class InterviewQuestion
{
    public string Id { get; set; } = "";
    public string Question { get; set; } = "";
    /// <summary>Markdown, revealed after the user answers.</summary>
    public string ModelAnswer { get; set; } = "";
}
