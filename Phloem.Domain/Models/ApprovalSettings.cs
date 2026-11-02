namespace Phloem.Domain.Models;

/// <summary>How long changes may wait for a second person (from the "Approvals" settings).</summary>
/// <param name="ProposalLifetime">How long a proposal (e.g. new Terms) waits before it expires (default 7 days).</param>
/// <param name="BlockConfirmationWindow">How long a shop block waits for confirmation before the shop is unblocked
/// automatically (default 24 hours).</param>
public sealed record ApprovalSettings(TimeSpan ProposalLifetime, TimeSpan BlockConfirmationWindow);
