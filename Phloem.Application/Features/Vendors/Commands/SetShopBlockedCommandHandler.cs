using MediatR;
using Phloem.Application.Features.Approvals.Common;
using Phloem.Application.Features.Vendors.Common;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Block: hidden from buyers, Seller role removed. Unblock: back to Approved (role restored) if it had been approved,
/// otherwise back to waiting for review. Shop and role change in ONE transaction.
/// </summary>
/// <remarks>
/// <para><b>Two-person rule:</b> a block applies AT ONCE (fraud can't wait), and a second person (another owner, or a
/// Country manager covering the shop's country) must confirm it on Approvals within the confirmation window
/// (default 24 hours). If nobody confirms, or the confirmation is rejected, the shop is unblocked automatically.</para>
/// <para>Unblocking by hand while a confirmation is waiting withdraws it. Blocking a shop that is already blocked
/// does nothing. Every block and unblock is in the audit log.</para>
/// </remarks>
public sealed class SetShopBlockedCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IPendingChangeRepository pendingChanges,
    IStaffAuditRepository audit,
    IApprovalSettingsProvider approvalSettings,
    ShopBlocking blocking,
    ApprovalEmails emails,
    TimeProvider time)
    : IRequestHandler<SetShopBlockedCommand, Result>
{
    public async Task<Result> Handle(SetShopBlockedCommand command, CancellationToken ct)
    {
        var actorId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByIdAsync(command.ShopId, ct);
        // Not found, or a shop in a country this person doesn't cover: answer the same way
        if (vendor is null || !currentUser.CoversCountry(vendor.Country)) return VendorErrors.NotFound;

        var isBlocked = vendor.Status == VendorStatus.Blocked;
        if (command.Blocked == isBlocked) return Result.Success();   // nothing to do

        var now = time.GetUtcNow().UtcDateTime;
        var waiting = await pendingChanges.GetWaitingForAsync(PendingChangeKind.ConfirmShopBlock, vendor.Id, ct);
        var target = $"Shop {vendor.Name}";

        // ───────────────────────────── Unblock ─────────────────────────────
        if (!command.Blocked)
        {
            return await blocking.SetAsync(vendor, blocked: false, () =>
            {
                waiting?.Withdraw(actorId, now, "The shop was unblocked by hand.");
                audit.Add(StaffAuditEntry.Create(actorId, StaffAction.ShopUnblocked, target, vendor.Country));
            }, ct);
        }

        // ───────────────────────────── Block (now) + confirmation ─────────────────────────────
        PendingChange? confirmation = null;
        var blockedNow = await blocking.SetAsync(vendor, blocked: true, () =>
        {
            audit.Add(StaffAuditEntry.Create(actorId, StaffAction.ShopBlocked, target, vendor.Country,
                                             "Waiting for a second person to confirm"));
            if (waiting is not null) return;   // one confirmation per block

            confirmation = PendingChange.Propose(
                PendingChangeKind.ConfirmShopBlock, $"Block {vendor.Name}", "{}", actorId, now,
                approvalSettings.Get().BlockConfirmationWindow, vendor.Country, vendor.Id);
            pendingChanges.Add(confirmation);
            audit.Add(StaffAuditEntry.Create(actorId, StaffAction.ChangeProposed, target, vendor.Country,
                                             "Block needs confirming"));
        }, ct);
        if (blockedNow.IsFailure) return blockedNow;

        if (confirmation is not null) await emails.WaitingAsync(confirmation, ct);
        return Result.Success();
    }
}
