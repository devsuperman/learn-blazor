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
    public List<ChoiceQuestion> Predict { get; set; } = [];
    public List<PracticeExercise> Practice { get; set; } = [];
    public List<ChoiceQuestion> Quiz { get; set; } = [];
    /// <summary>Minimum quiz score (percent) needed to pass the level.</summary>
    public int QuizPassPercent { get; set; } = 80;
    public List<InterviewQuestion> Interview { get; set; } = [];
}

/// <summary>A multiple-choice question, used by Predict and Quiz.</summary>
public sealed class ChoiceQuestion
{
    public string Id { get; set; } = "";
    /// <summary>Optional short heading (Predict uses it for the snippet name).</summary>
    public string? Title { get; set; }
    /// <summary>Markdown with the question (and the snippet, if any).</summary>
    public string Body { get; set; } = "";
    /// <summary>Markdown, one per option.</summary>
    public List<string> Options { get; set; } = [];
    /// <summary>Index into <see cref="Options"/>.</summary>
    public int Correct { get; set; }
    /// <summary>Markdown shown after the user picks an option.</summary>
    public string Explanation { get; set; } = "";
}

/// <summary>A coding exercise written in a textbox and checked with simple rules.</summary>
public sealed class PracticeExercise
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>Markdown.</summary>
    public string Task { get; set; } = "";
    /// <summary>Code pre-filled in the textbox.</summary>
    public string Starter { get; set; } = "";
    /// <summary>A model solution, shown only after the first check.</summary>
    public string Solution { get; set; } = "";
    /// <summary>Optional markdown hint, hidden until requested.</summary>
    public string? Hint { get; set; }
    public List<CodeCheck> Checks { get; set; } = [];
}

/// <summary>One rule run against the user's code: a regex that must (or must not) match.</summary>
public sealed class CodeCheck
{
    public string Id { get; set; } = "";
    /// <summary>What the rule checks, in plain words.</summary>
    public string Label { get; set; } = "";
    /// <summary>.NET regular expression.</summary>
    public string Pattern { get; set; } = "";
    /// <summary>When true, the check passes if the pattern does NOT match.</summary>
    public bool MustNotMatch { get; set; }
    /// <summary>Markdown shown when the check fails.</summary>
    public string? Hint { get; set; }
}

public sealed class InterviewQuestion
{
    public string Id { get; set; } = "";
    public string Question { get; set; } = "";
    /// <summary>Markdown, revealed after the user answers.</summary>
    public string ModelAnswer { get; set; } = "";
}
