namespace LearnBlazor.Models;

public enum TopicStatus { NotStarted, InProgress, Passed }

public enum ChallengeState { NotStarted, Running, Finished }

/// <summary>Everything the user did on one level. Stored as JSON in localStorage.</summary>
public sealed class LessonProgress
{
    public Dictionary<string, string> Predictions { get; set; } = [];
    public HashSet<string> RevealedPredictions { get; set; } = [];
    public HashSet<string> PracticeDone { get; set; } = [];
    public ChallengeState Challenge { get; set; } = ChallengeState.NotStarted;
    public int ChallengeAttempts { get; set; }
    public Dictionary<string, string> ChallengeAnswers { get; set; } = [];
    public HashSet<string> RubricMet { get; set; } = [];
    public Dictionary<string, string> InterviewAnswers { get; set; } = [];
    public HashSet<string> InterviewRevealed { get; set; } = [];
    public string NoteWhat { get; set; } = "";
    public string NoteDiffers { get; set; } = "";
    public string NoteGotcha { get; set; } = "";
    public string FreeNotes { get; set; } = "";
    public bool Passed { get; set; }

    /// <summary>The theory tab is locked while the challenge runs ("close the theory").</summary>
    public bool TheoryLocked => Challenge == ChallengeState.Running;

    public TopicStatus Status =>
        Passed ? TopicStatus.Passed
        : HasAnyActivity ? TopicStatus.InProgress
        : TopicStatus.NotStarted;

    private bool HasAnyActivity =>
        Predictions.Values.Any(v => !string.IsNullOrWhiteSpace(v))
        || PracticeDone.Count > 0
        || Challenge != ChallengeState.NotStarted
        || InterviewAnswers.Values.Any(v => !string.IsNullOrWhiteSpace(v))
        || !string.IsNullOrWhiteSpace(NoteWhat + NoteDiffers + NoteGotcha + FreeNotes);
}
