using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One AI call: which job and wording, what went in, what came back, what it cost, and later what staff decided.
/// </summary>
/// <remarks>
/// <para><b>Why:</b> (1) the monthly spend cap and the cost shown on Admin → AI; (2) checking how a job behaves;
/// (3) training data for Phloem's own model later: the AI's answer next to staff's final decision.</para>
/// <para><b>Privacy:</b> the input can hold people's text (shop descriptions, later messages), so rows are deleted
/// after <c>Ai:LogRetentionMonths</c> (default 12), only staff with <c>ai.prompts.manage</c> can open them, and every
/// open is audited.</para>
/// </remarks>
public class AiCallLog : Entity<Guid>
{
    /// <summary>Longest input or output kept (longer text is cut).</summary>
    public const int TextMaxLength = 20_000;

    /// <summary>Longest subject.</summary>
    public const int SubjectMaxLength = 200;

    /// <summary>Longest error message.</summary>
    public const int ErrorMaxLength = 500;

    /// <summary>Longest final decision.</summary>
    public const int DecisionMaxLength = 200;

    /// <summary>Longest model name.</summary>
    public const int ModelMaxLength = 100;

    /// <summary>For EF Core.</summary>
    protected AiCallLog() { }

    private AiCallLog(Guid id) : base(id) { }

    /// <summary>The job's prompt key.</summary>
    public string PromptKey { get; private set; } = string.Empty;

    /// <summary>The wording's version (0 = Phloem's default).</summary>
    public int PromptVersion { get; private set; }

    /// <summary>What it was about, e.g. "Shop Mama's Kitchen".</summary>
    public string? Subject { get; private set; }

    /// <summary>The provider used.</summary>
    public AiProvider Provider { get; private set; }

    /// <summary>The model used.</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>What was sent (after the instructions).</summary>
    public string Input { get; private set; } = string.Empty;

    /// <summary>What came back; null when the call failed.</summary>
    public string? Output { get; private set; }

    /// <summary>How it ended.</summary>
    public AiCallOutcome Outcome { get; private set; }

    /// <summary>Why it failed; null when it worked.</summary>
    public string? Error { get; private set; }

    /// <summary>Tokens sent.</summary>
    public int InputTokens { get; private set; }

    /// <summary>Tokens received.</summary>
    public int OutputTokens { get; private set; }

    /// <summary>What it cost (US dollars).</summary>
    public decimal CostUsd { get; private set; }

    /// <summary>How long the model took (milliseconds).</summary>
    public int DurationMs { get; private set; }

    /// <summary>What staff finally decided, e.g. "Approved", "Edited", "Kept the AI's text".</summary>
    public string? FinalDecision { get; private set; }

    /// <summary>Who decided.</summary>
    public Guid? DecidedBy { get; private set; }

    /// <summary>When they decided (UTC).</summary>
    public DateTime? DecidedAtUtc { get; private set; }

    /// <summary>Records a finished call (not yet saved). Long text is cut, never refused: logging must not fail.</summary>
    /// <param name="promptKey">The job's key.</param>
    /// <param name="promptVersion">The wording's version.</param>
    /// <param name="subject">What it was about.</param>
    /// <param name="provider">The provider.</param>
    /// <param name="model">The model.</param>
    /// <param name="input">What was sent.</param>
    /// <param name="output">What came back (null if nothing).</param>
    /// <param name="outcome">How it ended.</param>
    /// <param name="error">Why it failed.</param>
    /// <param name="inputTokens">Tokens sent.</param>
    /// <param name="outputTokens">Tokens received.</param>
    /// <param name="costUsd">The cost.</param>
    /// <param name="duration">How long it took.</param>
    /// <returns>The log row.</returns>
    public static AiCallLog Record(string promptKey, int promptVersion, string? subject, AiProvider provider,
                                   string model, string input, string? output, AiCallOutcome outcome, string? error,
                                   int inputTokens, int outputTokens, decimal costUsd, TimeSpan duration)
    {
        return new AiCallLog(Guid.CreateVersion7())
        {
            PromptKey = Cut(promptKey, AiPromptVersion.KeyMaxLength)!,
            PromptVersion = Math.Max(0, promptVersion),
            Subject = Cut(subject, SubjectMaxLength),
            Provider = provider,
            Model = Cut(model, ModelMaxLength) ?? string.Empty,
            Input = Cut(input, TextMaxLength) ?? string.Empty,
            Output = Cut(output, TextMaxLength),
            Outcome = outcome,
            Error = Cut(error, ErrorMaxLength),
            InputTokens = Math.Max(0, inputTokens),
            OutputTokens = Math.Max(0, outputTokens),
            CostUsd = Math.Max(0m, Math.Round(costUsd, 6)),
            DurationMs = (int)Math.Clamp(duration.TotalMilliseconds, 0, int.MaxValue)
        };
    }

    /// <summary>Records what staff finally decided about this answer (the label for training a model later).</summary>
    /// <param name="decision">E.g. "Approved" or "Edited".</param>
    /// <param name="decidedBy">Who decided.</param>
    /// <param name="utcNow">Now (UTC).</param>
    public void RecordDecision(string decision, Guid decidedBy, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(decision)) throw new ArgumentException("A decision is required.", nameof(decision));
        FinalDecision = Cut(decision.Trim(), DecisionMaxLength);
        DecidedBy = decidedBy;
        DecidedAtUtc = utcNow;
    }

    private static string? Cut(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
