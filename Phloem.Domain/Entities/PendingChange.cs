using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// A sensitive change that needs a second person: proposed by one staff member, approved or rejected by another.
/// </summary>
/// <remarks>
/// <para><b>Two kinds so far</b> (<see cref="PendingChangeKind"/>): publishing Terms / Privacy (applied only on
/// approval) and confirming a shop block (applied at once; undone if not confirmed in time).</para>
/// <para><b>Nobody approves their own:</b> <see cref="Approve"/> and <see cref="Reject"/> refuse the proposer.
/// Who else may decide (owners, or that country's Country manager) is checked by the Application layer.</para>
/// <para><b>Payload</b> is the change itself as JSON (e.g. the full Terms wording), so the approver sees and approves
/// exactly what will be applied, and nothing can change in between.</para>
/// <para>Never deleted: approved, rejected, withdrawn and expired changes stay as history.
/// <see cref="Entity{TId}.CreatedAt"/> is when it was proposed.</para>
/// </remarks>
public class PendingChange : Entity<Guid>
{
    /// <summary>Longest <see cref="Summary"/>.</summary>
    public const int SummaryMaxLength = 300;

    /// <summary>Longest <see cref="DecisionNote"/> (a rejection reason).</summary>
    public const int DecisionNoteMaxLength = 500;

    /// <summary>Longest <see cref="Outcome"/>.</summary>
    public const int OutcomeMaxLength = 500;

    /// <summary>For EF Core.</summary>
    protected PendingChange() { }

    private PendingChange(Guid id) : base(id) { }

    /// <summary>What kind of change it is.</summary>
    public PendingChangeKind Kind { get; private set; }

    /// <summary>Where it stands.</summary>
    public PendingChangeStatus Status { get; private set; }

    /// <summary>The country it concerns; null = Phloem-wide (e.g. Terms).</summary>
    public Country? Country { get; private set; }

    /// <summary>The record it concerns (e.g. the blocked shop); null when none.</summary>
    public Guid? TargetId { get; private set; }

    /// <summary>What it is, for people, e.g. "Publish the Terms &amp; Conditions" or "Block Mama's Kitchen".</summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>The change itself as JSON (empty "{}" when the target is enough).</summary>
    public string Payload { get; private set; } = "{}";

    /// <summary>Who proposed it.</summary>
    public Guid ProposedById { get; private set; }

    /// <summary>After this moment it can no longer be approved (UTC).</summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>Who approved, rejected or withdrew it.</summary>
    public Guid? DecidedById { get; private set; }

    /// <summary>When it was decided, withdrawn or expired (UTC).</summary>
    public DateTime? DecidedAtUtc { get; private set; }

    /// <summary>Why it was rejected or withdrawn.</summary>
    public string? DecisionNote { get; private set; }

    /// <summary>What happened when it was applied, e.g. "Published as version 6", or why it failed.</summary>
    public string? Outcome { get; private set; }

    /// <summary>True while a second person can still decide.</summary>
    /// <param name="utcNow">The current time.</param>
    public bool IsWaitingAt(DateTime utcNow) => Status == PendingChangeStatus.Waiting && utcNow < ExpiresAtUtc;

    /// <summary>Proposes a change.</summary>
    /// <param name="kind">What kind of change.</param>
    /// <param name="summary">What it is, for people (cut to <see cref="SummaryMaxLength"/>).</param>
    /// <param name="payload">The change as JSON.</param>
    /// <param name="proposedById">Who proposes it.</param>
    /// <param name="utcNow">The current time.</param>
    /// <param name="lifetime">How long it can wait for a second person.</param>
    /// <param name="country">The country it concerns; null = Phloem-wide.</param>
    /// <param name="targetId">The record it concerns.</param>
    /// <returns>The new proposal (not yet saved).</returns>
    /// <exception cref="ArgumentException">Missing proposer, summary or payload.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown kind or country, or a lifetime that isn't positive.</exception>
    public static PendingChange Propose(PendingChangeKind kind, string summary, string payload, Guid proposedById,
                                        DateTime utcNow, TimeSpan lifetime, Country? country = null,
                                        Guid? targetId = null)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (country is { } c && !Enum.IsDefined(c)) throw new ArgumentOutOfRangeException(nameof(country));
        if (proposedById == Guid.Empty) throw new ArgumentException("Who proposes it is required.", nameof(proposedById));
        if (string.IsNullOrWhiteSpace(summary)) throw new ArgumentException("A summary is required.", nameof(summary));
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("The change is required.", nameof(payload));
        if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));

        return new PendingChange(Guid.CreateVersion7())
        {
            Kind = kind,
            Status = PendingChangeStatus.Waiting,
            Country = country,
            TargetId = targetId,
            Summary = Cut(summary.Trim(), SummaryMaxLength),
            Payload = payload,
            ProposedById = proposedById,
            ExpiresAtUtc = utcNow + lifetime
        };
    }

    /// <summary>A second person approves it; <paramref name="outcome"/> says what was applied.</summary>
    /// <param name="deciderId">Who approves (not the proposer).</param>
    /// <param name="utcNow">The current time.</param>
    /// <param name="outcome">What happened, e.g. "Published as version 6".</param>
    /// <exception cref="InvalidOperationException">Not waiting any more, expired, or the proposer approving their
    /// own (the Application layer checks first, so this is a safety net).</exception>
    public void Approve(Guid deciderId, DateTime utcNow, string? outcome)
    {
        EnsureDecidable(deciderId, utcNow);
        Decide(PendingChangeStatus.Approved, deciderId, utcNow, note: null, outcome);
    }

    /// <summary>Approved, but applying it failed: nothing changed, and the reason is kept.</summary>
    /// <param name="deciderId">Who approved it.</param>
    /// <param name="utcNow">The current time.</param>
    /// <param name="reason">Why it failed.</param>
    public void Fail(Guid deciderId, DateTime utcNow, string reason)
    {
        EnsureDecidable(deciderId, utcNow);
        Decide(PendingChangeStatus.Failed, deciderId, utcNow, note: null, reason);
    }

    /// <summary>A second person rejects it.</summary>
    /// <param name="deciderId">Who rejects (not the proposer).</param>
    /// <param name="utcNow">The current time.</param>
    /// <param name="reason">Why (shown to the proposer).</param>
    /// <param name="outcome">What happened, e.g. "The shop was unblocked".</param>
    /// <exception cref="InvalidOperationException">Not waiting any more, expired, or the proposer rejecting their own.</exception>
    public void Reject(Guid deciderId, DateTime utcNow, string reason, string? outcome = null)
    {
        EnsureDecidable(deciderId, utcNow);
        Decide(PendingChangeStatus.Rejected, deciderId, utcNow, reason, outcome);
    }

    /// <summary>Taken back (by the proposer), replaced by a newer proposal, or overtaken (e.g. the shop was
    /// unblocked by hand). Does nothing when it's no longer waiting.</summary>
    /// <param name="byId">Who withdrew it.</param>
    /// <param name="utcNow">The current time.</param>
    /// <param name="reason">Why.</param>
    public void Withdraw(Guid byId, DateTime utcNow, string reason)
    {
        if (Status != PendingChangeStatus.Waiting) return;
        Decide(PendingChangeStatus.Withdrawn, byId, utcNow, reason, outcome: null);
    }

    /// <summary>Nobody decided in time. Does nothing when it's no longer waiting.</summary>
    /// <param name="utcNow">The current time.</param>
    /// <param name="outcome">What happened, e.g. "The shop was unblocked automatically".</param>
    public void Expire(DateTime utcNow, string? outcome = null)
    {
        if (Status != PendingChangeStatus.Waiting) return;
        Status = PendingChangeStatus.Expired;
        DecidedAtUtc = utcNow;
        Outcome = outcome is null ? null : Cut(outcome, OutcomeMaxLength);
    }

    private void EnsureDecidable(Guid deciderId, DateTime utcNow)
    {
        if (!IsWaitingAt(utcNow)) throw new InvalidOperationException("This change is no longer waiting.");
        if (deciderId == Guid.Empty) throw new InvalidOperationException("Who decides is required.");
        if (deciderId == ProposedById) throw new InvalidOperationException("Nobody can decide their own change.");
    }

    private void Decide(PendingChangeStatus status, Guid byId, DateTime utcNow, string? note, string? outcome)
    {
        Status = status;
        DecidedById = byId;
        DecidedAtUtc = utcNow;
        DecisionNote = string.IsNullOrWhiteSpace(note) ? null : Cut(note.Trim(), DecisionNoteMaxLength);
        Outcome = string.IsNullOrWhiteSpace(outcome) ? null : Cut(outcome.Trim(), OutcomeMaxLength);
    }

    private static string Cut(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
}
