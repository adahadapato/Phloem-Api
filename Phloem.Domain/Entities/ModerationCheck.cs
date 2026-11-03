using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One check of people's text: a dish's wording, a shop name, a suggested dish name or a Contact message.
/// </summary>
/// <remarks>
/// <para><b>Flow:</b> saved as <see cref="ModerationState.WaitingForAi"/> → the background checker asks the AI
/// (within about a minute) → <see cref="ModerationState.DoneByAi"/>, or <see cref="ModerationState.WaitingForPerson"/>
/// when the AI flagged it or couldn't check it → a person approves or rejects it on Admin → Moderation.</para>
/// <para><b>Text</b> is a copy of what was checked, so a later edit doesn't change what the decision was about; if
/// the text changes before the check finishes, the check is <see cref="ModerationState.Replaced"/>.</para>
/// <para>Kept as a record (and, with the AI log, as training data for Phloem's own model later).</para>
/// </remarks>
public class ModerationCheck : Entity<Guid>
{
    /// <summary>Longest text kept.</summary>
    public const int TextMaxLength = 4000;

    /// <summary>Longest reason, rule or note.</summary>
    public const int ReasonMaxLength = 500;

    /// <summary>Longest short label (rule, close match, topic).</summary>
    public const int LabelMaxLength = 120;

    /// <summary>How many times the AI may fail (service down, unusable answer) before a person checks it.</summary>
    public const int MaxAiAttempts = 3;

    /// <summary>For EF Core.</summary>
    protected ModerationCheck() { }

    private ModerationCheck(Guid id) : base(id) { }

    /// <summary>What it is.</summary>
    public ModerationKind Kind { get; private set; }

    /// <summary>The dish, shop, dish name or message.</summary>
    public Guid TargetId { get; private set; }

    /// <summary>The country (for the queue: staff see their countries); null for Contact messages.</summary>
    public Country? Country { get; private set; }

    /// <summary>The text checked (a copy).</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>Where it is.</summary>
    public ModerationState State { get; private set; }

    /// <summary>The AI's verdict; null until it answers (or when it couldn't).</summary>
    public ModerationVerdict? AiVerdict { get; private set; }

    /// <summary>The AI's reason, or why it couldn't check ("AI is off").</summary>
    public string? AiReason { get; private set; }

    /// <summary>Short label from the AI: the rule broken, the close match, or the topic.</summary>
    public string? AiLabel { get; private set; }

    /// <summary>Contact messages: the AI thinks it's spam.</summary>
    public bool? IsSpam { get; private set; }

    /// <summary>Contact messages: the AI thinks it's urgent.</summary>
    public bool? IsUrgent { get; private set; }

    /// <summary>The AI call (in the AI log).</summary>
    public Guid? AiCallId { get; private set; }

    /// <summary>How many times the AI was asked.</summary>
    public int AiAttempts { get; private set; }

    /// <summary>The person's decision (approved = <see cref="ModerationVerdict.Ok"/>).</summary>
    public ModerationVerdict? StaffVerdict { get; private set; }

    /// <summary>Why the person rejected it (shown to the seller for dishes).</summary>
    public string? StaffNote { get; private set; }

    /// <summary>Who decided.</summary>
    public Guid? DecidedBy { get; private set; }

    /// <summary>When (UTC).</summary>
    public DateTime? DecidedAtUtc { get; private set; }

    /// <summary>A new check, waiting for the AI.</summary>
    /// <param name="kind">What it is.</param>
    /// <param name="targetId">The dish, shop, name or message.</param>
    /// <param name="text">The text to check (cut to <see cref="TextMaxLength"/>).</param>
    /// <param name="country">The country, when there is one.</param>
    public static ModerationCheck Queue(ModerationKind kind, Guid targetId, string text, Country? country)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (targetId == Guid.Empty) throw new ArgumentException("A target is required.", nameof(targetId));

        return new ModerationCheck(Guid.CreateVersion7())
        {
            Kind = kind,
            TargetId = targetId,
            Country = country,
            Text = Cut(text ?? string.Empty, TextMaxLength)!,
            State = ModerationState.WaitingForAi
        };
    }

    /// <summary>The AI answered.</summary>
    /// <param name="verdict">Its verdict.</param>
    /// <param name="reason">Why.</param>
    /// <param name="label">Rule, close match or topic.</param>
    /// <param name="aiCallId">The AI call.</param>
    /// <param name="needsPerson">True to put it in the queue (flagged, or "hold everything" is on).</param>
    public void AnsweredByAi(ModerationVerdict verdict, string? reason, string? label, Guid aiCallId, bool needsPerson)
    {
        AiAttempts++;
        AiVerdict = verdict;
        AiReason = Cut(reason, ReasonMaxLength);
        AiLabel = Cut(label, LabelMaxLength);
        AiCallId = aiCallId;
        State = needsPerson ? ModerationState.WaitingForPerson : ModerationState.DoneByAi;
    }

    /// <summary>Contact messages: what the AI made of it.</summary>
    public void SortedByAi(bool spam, bool urgent, string? summary, string? topic, Guid aiCallId)
    {
        AiAttempts++;
        IsSpam = spam;
        IsUrgent = urgent;
        AiReason = Cut(summary, ReasonMaxLength);
        AiLabel = Cut(topic, LabelMaxLength);
        AiVerdict = spam ? ModerationVerdict.NotAllowed : ModerationVerdict.Ok;
        AiCallId = aiCallId;
        State = ModerationState.DoneByAi;
    }

    /// <summary>The AI failed this time (service down, unusable answer).</summary>
    /// <param name="why">What went wrong.</param>
    /// <returns>True when it has failed <see cref="MaxAiAttempts"/> times (give up).</returns>
    public bool AiFailed(string why)
    {
        AiAttempts++;
        AiReason = Cut(why, ReasonMaxLength);
        return AiAttempts >= MaxAiAttempts;
    }

    /// <summary>The AI couldn't check it (off, spend cap, failed too often).</summary>
    /// <param name="why">Shown on the queue, e.g. "AI is off".</param>
    /// <param name="needsPerson">True to put it in the queue; false to finish it unchecked (advice only).</param>
    public void NotCheckedByAi(string why, bool needsPerson)
    {
        AiReason = Cut(why, ReasonMaxLength);
        State = needsPerson ? ModerationState.WaitingForPerson : ModerationState.DoneByAi;
    }

    /// <summary>A person decided.</summary>
    /// <param name="approved">True = fine.</param>
    /// <param name="note">Why (required when rejected).</param>
    /// <param name="staffId">Who.</param>
    /// <param name="utcNow">Now (UTC).</param>
    public void Decide(bool approved, string? note, Guid staffId, DateTime utcNow)
    {
        if (!approved && string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("A reason is required to reject.", nameof(note));

        StaffVerdict = approved ? ModerationVerdict.Ok : ModerationVerdict.NotAllowed;
        StaffNote = Cut(note?.Trim(), ReasonMaxLength);
        DecidedBy = staffId;
        DecidedAtUtc = utcNow;
        State = ModerationState.DecidedByStaff;
    }

    /// <summary>The text changed or was deleted before the check finished.</summary>
    public void Replace() => State = ModerationState.Replaced;

    private static string? Cut(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
