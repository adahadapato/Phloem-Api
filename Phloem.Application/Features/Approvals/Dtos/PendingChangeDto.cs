using Phloem.Application.Features.Approvals.Common;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Approvals.Dtos;

/// <summary>One change on Admin → Approvals.</summary>
/// <param name="Id">The change.</param>
/// <param name="Kind">What kind of change.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="Summary">What it is, e.g. "Publish the Terms &amp; Conditions".</param>
/// <param name="Country">The country it concerns; null = Phloem-wide.</param>
/// <param name="TargetId">The record it concerns (e.g. the shop).</param>
/// <param name="ProposedByName">Who proposed it.</param>
/// <param name="ProposedAt">When (UTC).</param>
/// <param name="ExpiresAt">When it stops waiting (UTC).</param>
/// <param name="DecidedByName">Who decided it, if anyone.</param>
/// <param name="DecidedAt">When it was decided or expired (UTC).</param>
/// <param name="DecisionNote">Why it was rejected or withdrawn.</param>
/// <param name="Outcome">What happened when it was applied.</param>
/// <param name="CanDecide">True when the viewer may approve or reject it now.</param>
/// <param name="CanWithdraw">True when the viewer may take it back now.</param>
/// <param name="IsYours">True when the viewer proposed it.</param>
/// <param name="Legal">For Terms / Privacy: exactly what will be published.</param>
public sealed record PendingChangeDto(
    Guid Id,
    PendingChangeKind Kind,
    PendingChangeStatus Status,
    string Summary,
    Country? Country,
    Guid? TargetId,
    string ProposedByName,
    DateTime ProposedAt,
    DateTime ExpiresAt,
    string? DecidedByName,
    DateTime? DecidedAt,
    string? DecisionNote,
    string? Outcome,
    bool CanDecide,
    bool CanWithdraw,
    bool IsYours,
    LegalDocumentProposal? Legal)
{
    /// <summary>Builds the line for one viewer.</summary>
    /// <param name="change">The change.</param>
    /// <param name="names">Person → name, for the proposer and decider.</param>
    /// <param name="canDecide">The viewer may decide it now.</param>
    /// <param name="canWithdraw">The viewer may take it back now.</param>
    /// <param name="viewerId">The viewer.</param>
    public static PendingChangeDto From(PendingChange change, IReadOnlyDictionary<Guid, string> names,
                                        bool canDecide, bool canWithdraw, Guid? viewerId) => new(
        change.Id, change.Kind, change.Status, change.Summary, change.Country, change.TargetId,
        names.GetValueOrDefault(change.ProposedById, "Unknown user"),
        change.CreatedAt, change.ExpiresAtUtc,
        change.DecidedById is { } d ? names.GetValueOrDefault(d, "Unknown user") : null,
        change.DecidedAtUtc, change.DecisionNote, change.Outcome,
        canDecide, canWithdraw, change.ProposedById == viewerId,
        change.Kind == PendingChangeKind.PublishLegalDocument ? LegalDocumentProposal.FromJson(change.Payload) : null);
}
