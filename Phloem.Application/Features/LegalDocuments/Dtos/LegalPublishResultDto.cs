namespace Phloem.Application.Features.LegalDocuments.Dtos;

/// <summary>What happened when someone pressed Publish.</summary>
/// <param name="Version">The new version number when it was published now (Help, or an approved proposal); null
/// while it waits for approval.</param>
/// <param name="WaitingForApproval">True for Terms and Privacy: another owner must approve it on Approvals first.</param>
/// <param name="ApprovalId">The waiting change, when <paramref name="WaitingForApproval"/>.</param>
/// <param name="Message">What to tell the person, e.g. "Sent for approval…".</param>
public sealed record LegalPublishResultDto(int? Version, bool WaitingForApproval, Guid? ApprovalId, string Message);
