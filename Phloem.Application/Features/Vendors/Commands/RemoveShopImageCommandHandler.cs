using MediatR;
using Phloem.Domain.Enums;                                                    // NEW: ShopImage lives here now
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Removes the shop's logo or cover photo (<see cref="ShopImage"/> says which), saves, then deletes its file.
/// Removing one that isn't there is fine.
/// </summary>
public sealed class RemoveShopImageCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveShopImageCommand, Result>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(RemoveShopImageCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var old = command.Image == ShopImage.Logo ? vendor.RemoveLogo() : vendor.RemoveCoverPhoto();
        if (old is null) return Result.Success();

        await unitOfWork.SaveChangesAsync(ct);
        await storage.DeleteAsync(old.Path, StorageContainer.Public, ct);
        return Result.Success();
    }
}