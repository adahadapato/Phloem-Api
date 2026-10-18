using MediatR;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Block: hidden from buyers, Seller role removed. Unblock: back to Approved (role restored) if it had been approved,
/// otherwise back to waiting for review. Shop and role change in ONE transaction.
/// </summary>
public sealed class SetShopBlockedCommandHandler(
    IVendorRepository vendors,
    IUserRoleService roles,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetShopBlockedCommand, Result>
{
    public async Task<Result> Handle(SetShopBlockedCommand command, CancellationToken ct)
    {
        var vendor = await vendors.GetByIdAsync(command.ShopId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        if (command.Blocked) vendor.Block();
        else vendor.Unblock();

        await unitOfWork.SaveChangesAsync(ct);

        var role = vendor.Status == VendorStatus.Approved
            ? await roles.AddToRoleAsync(vendor.OwnerId, Roles.Seller, ct)
            : await roles.RemoveFromRoleAsync(vendor.OwnerId, Roles.Seller, ct);
        if (role.IsFailure) return role;   // no commit = rollback

        await transaction.CommitAsync(ct);
        return Result.Success();
    }
}
