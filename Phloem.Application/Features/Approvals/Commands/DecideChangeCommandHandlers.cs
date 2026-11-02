using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Application.Common.Approvals;
using Phloem.Application.Common.Authorization;
using Phloem.Application.Features.Approvals.Common;
using Phloem.Application.Features.LegalDocuments.Commands;
using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Application.Features.Vendors.Common;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Approvals.Commands;

/// <summary>
/// Approve, reject and withdraw on Admin → Approvals, and the expiry task.
/// </summary>
/// <remarks>
/// <para>Every decision checks again, at the moment it's made: still waiting, not your own, and you may decide it
/// (<see cref="ApprovalRules"/>). Every decision is in the audit log and emailed to the proposer.</para>
/// <para><b>Terms / Privacy:</b> approving publishes exactly the stored wording (through the normal publish command,
/// marked as approved via <see cref="ApprovalContext"/>), in the same transaction as marking it approved.</para>
/// <para><b>Shop block:</b> approving keeps the block; rejecting or expiring unblocks the shop.</para>
/// </remarks>
public sealed class DecideChangeCommandHandlers(
    ICurrentUser currentUser,
    IPendingChangeRepository changes,
    IVendorRepository vendors,
    IStaffAuditRepository audit,
    ShopBlocking blocking,
    ApprovalContext approvals,
    ApprovalEmails emails,
    ISender sender,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<DecideChangeCommandHandlers> logger)
    : IRequestHandler<ApproveChangeCommand, Result<string>>,
      IRequestHandler<RejectChangeCommand, Result>,
      IRequestHandler<WithdrawChangeCommand, Result>,
      IRequestHandler<ExpirePendingChangesCommand, Result<int>>
{
    // ───────────────────────────── Approve ─────────────────────────────

    /// <summary>Approves and applies the change.</summary>
    public async Task<Result<string>> Handle(ApproveChangeCommand command, CancellationToken ct)
    {
        var (found, error) = await LoadDecidableAsync(command.Id, ct);
        if (error is not null || found is null) return error ?? ApprovalErrors.NotFound;
        var change = found;

        var me = currentUser.UserId!.Value;
        var now = time.GetUtcNow().UtcDateTime;

        switch (change.Kind)
        {
            case PendingChangeKind.PublishLegalDocument:
                return await ApproveLegalAsync(change, me, now, ct);

            case PendingChangeKind.ConfirmShopBlock:
            {
                var vendor = change.TargetId is { } shopId ? await vendors.GetByIdAsync(shopId, ct) : null;
                if (vendor is null || vendor.Status != VendorStatus.Blocked)
                {
                    change.Withdraw(me, now, "The shop is no longer blocked.");
                    await unitOfWork.SaveChangesAsync(ct);
                    return ApprovalErrors.NoLongerWaiting;
                }

                change.Approve(me, now, "The block is confirmed.");
                Audit(StaffAction.ChangeApproved, change);
                await unitOfWork.SaveChangesAsync(ct);
                await emails.DecidedAsync(change, ct);
                return "The block is confirmed.";
            }

            default:
                return ApprovalErrors.NotAllowed;
        }
    }

    private async Task<Result<string>> ApproveLegalAsync(PendingChange change, Guid me, DateTime now,
                                                         CancellationToken ct)
    {
        var proposal = LegalDocumentProposal.FromJson(change.Payload);
        if (proposal is null)
        {
            change.Fail(me, now, "The stored wording couldn't be read.");
            await unitOfWork.SaveChangesAsync(ct);
            return ApprovalErrors.NoLongerWaiting;
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        Result<LegalPublishResultDto> published;
        using (approvals.Applying(change))
            published = await sender.Send(new PublishLegalDocumentCommand(proposal.Type, proposal.Title,
                                                                          proposal.Content, proposal.IsDraft), ct);

        if (published.IsFailure)
        {
            // Nothing was published; keep the reason on the change
            change.Fail(me, now, published.Error!.Message);
            Audit(StaffAction.ChangeApproved, change, "Couldn't be applied");
            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            await emails.DecidedAsync(change, ct);
            return published.Error;
        }

        var outcome = $"Published as version {published.Value!.Version}.";
        change.Approve(me, now, outcome);
        Audit(StaffAction.ChangeApproved, change, outcome);
        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogWarning("{ApproverId} approved change {ChangeId} ({Summary}): {Outcome}",
                          me, change.Id, change.Summary, outcome);
        await emails.DecidedAsync(change, ct);
        return outcome;
    }

    // ───────────────────────────── Reject ─────────────────────────────

    /// <summary>Rejects the change; a rejected block unblocks the shop.</summary>
    public async Task<Result> Handle(RejectChangeCommand command, CancellationToken ct)
    {
        var (found, error) = await LoadDecidableAsync(command.Id, ct);
        if (error is not null || found is null) return error ?? ApprovalErrors.NotFound;
        var change = found;

        var me = currentUser.UserId!.Value;
        var now = time.GetUtcNow().UtcDateTime;
        var reason = command.Reason.Trim();

        if (change.Kind == PendingChangeKind.ConfirmShopBlock
            && change.TargetId is { } shopId
            && await vendors.GetByIdAsync(shopId, ct) is { Status: VendorStatus.Blocked } vendor)
        {
            var unblocked = await blocking.SetAsync(vendor, blocked: false, () =>
            {
                change.Reject(me, now, reason, "The shop was unblocked.");
                Audit(StaffAction.ChangeRejected, change, reason);
                audit.Add(StaffAuditEntry.Create(me, StaffAction.ShopUnblocked, $"Shop {vendor.Name}", vendor.Country,
                                                 "Block not confirmed"));
            }, ct);
            if (unblocked.IsFailure) return unblocked;
        }
        else
        {
            change.Reject(me, now, reason);
            Audit(StaffAction.ChangeRejected, change, reason);
            await unitOfWork.SaveChangesAsync(ct);
        }

        await emails.DecidedAsync(change, ct);
        return Result.Success();
    }

    // ───────────────────────────── Withdraw ─────────────────────────────

    /// <summary>The proposer takes back their waiting Terms / Privacy proposal.</summary>
    public async Task<Result> Handle(WithdrawChangeCommand command, CancellationToken ct)
    {
        if (!currentUser.IsStaffMember()) return ApprovalErrors.StaffOnly;

        var change = await changes.GetByIdAsync(command.Id, ct);
        if (change is null || !ApprovalRules.CanSee(currentUser, change)) return ApprovalErrors.NotFound;

        var now = time.GetUtcNow().UtcDateTime;
        if (!change.IsWaitingAt(now)) return ApprovalErrors.NoLongerWaiting;
        if (!ApprovalRules.CanWithdraw(currentUser, change)) return ApprovalErrors.CannotWithdraw;

        var me = currentUser.UserId!.Value;
        change.Withdraw(me, now, "Taken back by the person who proposed it.");
        Audit(StaffAction.ChangeWithdrawn, change);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ───────────────────────────── Expire (background task) ─────────────────────────────

    /// <summary>Expires every change whose time is up; a block nobody confirmed is undone.</summary>
    public async Task<Result<int>> Handle(ExpirePendingChangesCommand command, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var expired = await changes.GetExpiredAsync(now, ct);
        if (expired.Count == 0) return 0;

        foreach (var change in expired)
        {
            if (change.Kind == PendingChangeKind.ConfirmShopBlock
                && change.TargetId is { } shopId
                && await vendors.GetByIdAsync(shopId, ct) is { Status: VendorStatus.Blocked } vendor)
            {
                var unblocked = await blocking.SetAsync(vendor, blocked: false,
                    () => change.Expire(now, "Nobody confirmed the block in time, so the shop was unblocked automatically."),
                    ct);
                if (unblocked.IsFailure)
                    logger.LogError("Couldn't unblock shop {ShopId} after its block expired: {Error}",
                                    vendor.Id, unblocked.Error!.Message);
            }
            else
            {
                change.Expire(now, change.Kind == PendingChangeKind.ConfirmShopBlock
                    ? "The shop had already been unblocked."
                    : "Nobody decided in time, so nothing was published.");
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Expired {Count} change(s) nobody decided in time.", expired.Count);

        foreach (var change in expired) await emails.DecidedAsync(change, ct);
        return expired.Count;
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    /// <summary>The change, when this person may decide it now; otherwise the error to return.</summary>
    private async Task<(PendingChange? Change, Error? Error)> LoadDecidableAsync(Guid id, CancellationToken ct)
    {
        if (!currentUser.IsStaffMember()) return (null, ApprovalErrors.StaffOnly);

        var change = await changes.GetByIdAsync(id, ct);
        if (change is null || !ApprovalRules.CanSee(currentUser, change)) return (null, ApprovalErrors.NotFound);
        if (!change.IsWaitingAt(time.GetUtcNow().UtcDateTime)) return (null, ApprovalErrors.NoLongerWaiting);
        if (change.ProposedById == currentUser.UserId) return (null, ApprovalErrors.OwnChange);
        if (!ApprovalRules.CanDecide(currentUser, change)) return (null, ApprovalErrors.NotAllowed);
        return (change, null);
    }

    private void Audit(StaffAction action, PendingChange change, string? detail = null) =>
        audit.Add(StaffAuditEntry.Create(currentUser.UserId!.Value, action, change.Summary, change.Country,
                                         detail));
}
