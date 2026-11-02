using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Common;

/// <summary>
/// Blocks or unblocks a shop with everything that goes with it, in ONE transaction: the shop's status, the owner's
/// Seller role (removed while blocked; back when an approved shop is unblocked), and any extra rows the caller adds
/// (audit lines, the approval record).
/// </summary>
/// <remarks>Used by Admin → Shops (block / unblock), by a rejected block confirmation, and by the expiry task (a block
/// nobody confirmed in time).</remarks>
public sealed class ShopBlocking(IUserRoleService roles, IUnitOfWork unitOfWork)
{
    /// <summary>Blocks or unblocks the shop and saves.</summary>
    /// <param name="vendor">The shop (tracked).</param>
    /// <param name="blocked">True to block, false to unblock.</param>
    /// <param name="alsoSave">Extra changes to save in the same transaction (e.g. audit lines); may be null.</param>
    /// <param name="ct">Stops the work.</param>
    /// <returns>Success; or the role change's error (then nothing is saved).</returns>
    public async Task<Result> SetAsync(Vendor vendor, bool blocked, Action? alsoSave, CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        if (blocked) vendor.Block();
        else vendor.Unblock();
        alsoSave?.Invoke();

        await unitOfWork.SaveChangesAsync(ct);

        var role = vendor.Status == VendorStatus.Approved
            ? await roles.AddToRoleAsync(vendor.OwnerId, Roles.Seller, ct)
            : await roles.RemoveFromRoleAsync(vendor.OwnerId, Roles.Seller, ct);
        if (role.IsFailure) return role;   // no commit = rollback

        await transaction.CommitAsync(ct);
        return Result.Success();
    }
}
