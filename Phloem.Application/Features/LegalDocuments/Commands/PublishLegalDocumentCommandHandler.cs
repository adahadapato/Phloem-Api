using MediatR;
using Phloem.Application.Common.Approvals;
using Phloem.Application.Features.Approvals.Common;
using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Commands;

/// <summary>Handles <see cref="PublishLegalDocumentCommand"/>.</summary>
/// <remarks>
/// <para><b>Help:</b> published at once, as before.</para>
/// <para><b>Terms / Privacy:</b> saved as a proposal for another owner to approve (the full wording is kept with it,
/// so exactly that is published). A newer proposal for the same document replaces a waiting older one. When the
/// approve handler applies it (<see cref="ApprovalContext"/>), it's published here like Help.</para>
/// <para>The version number is worked out when it's published, so approving in any order is safe.</para>
/// </remarks>
public sealed class PublishLegalDocumentCommandHandler(
    ICurrentUser currentUser,
    ILegalDocumentRepository documents,
    IPendingChangeRepository pendingChanges,
    IStaffAuditRepository audit,
    IApprovalSettingsProvider approvalSettings,
    ApprovalContext approvals,
    ApprovalEmails emails,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<PublishLegalDocumentCommand, Result<LegalPublishResultDto>>
{
    public async Task<Result<LegalPublishResultDto>> Handle(PublishLegalDocumentCommand command, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        // ───────────────────────────── Needs a second person ─────────────────────────────
        if (ApprovalRules.NeedsApproval(command.Type) && !approvals.IsApplying(PendingChangeKind.PublishLegalDocument))
        {
            var actorId = currentUser.UserId
                ?? throw new InvalidOperationException("This action requires a signed-in user.");
            var name = ApprovalRules.DocumentName(command.Type);

            // A newer proposal for the same document replaces a waiting older one
            foreach (var older in await pendingChanges.GetWaitingOfKindAsync(PendingChangeKind.PublishLegalDocument, ct))
                if (LegalDocumentProposal.FromJson(older.Payload)?.Type == command.Type)
                    older.Withdraw(actorId, now, "Replaced by a newer proposal.");

            var proposal = new LegalDocumentProposal(command.Type, command.Title.Trim(), command.Content.Trim(),
                                                     command.IsDraft);
            var change = PendingChange.Propose(
                PendingChangeKind.PublishLegalDocument,
                $"Publish the {name}" + (command.IsDraft ? " (draft wording)" : string.Empty),
                proposal.ToJson(), actorId, now, approvalSettings.Get().ProposalLifetime);

            pendingChanges.Add(change);
            audit.Add(StaffAuditEntry.Create(actorId, StaffAction.ChangeProposed, $"{name} (new version)",
                                             detail: proposal.Title));
            await unitOfWork.SaveChangesAsync(ct);

            await emails.WaitingAsync(change, ct);
            return new LegalPublishResultDto(null, true, change.Id,
                $"Sent for approval. The new {name} goes live once another owner approves it on Approvals.");
        }

        // ───────────────────────────── Publish now ─────────────────────────────
        var next = await documents.GetLatestVersionAsync(command.Type, ct) + 1;
        var document = LegalDocument.Publish(command.Type, next, command.Title, command.Content, command.IsDraft, now);
        documents.Add(document);
        await unitOfWork.SaveChangesAsync(ct);

        return new LegalPublishResultDto(next, false, null, $"Published as version {next}.");
    }
}
