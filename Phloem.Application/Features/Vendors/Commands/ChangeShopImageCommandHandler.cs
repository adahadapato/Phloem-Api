using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Changes the shop's logo or cover photo (<see cref="ShopImage"/> says which).
/// Simple processing (no enhancement), public container.
/// Order: process → save new file → change picture → save DB → delete OLD file
/// (shop pictures aren't copied into orders, so the old file can go).
/// </summary>
public sealed class ChangeShopImageCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IImageProcessor imageProcessor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeShopImageCommand, Result<string>>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<string>> Handle(ChangeShopImageCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;
        if (vendor.Status == VendorStatus.Blocked) return VendorErrors.Blocked;

        var processed = await imageProcessor.ProcessSimpleImageAsync(command.Content, ct);
        if (processed.IsFailure) return processed.Error;
        await using var image = processed.Value;

        var name = command.Image == ShopImage.Logo ? "logo" : "cover-photo";   // CHANGED: was "cover"
        var path = $"vendors/{vendor.Id:N}/{name}-{Guid.CreateVersion7():N}.webp";
        await storage.SaveAsync(image.FullSize, path, image.ContentType, StorageContainer.Public, ct);

        var picture = ImageFile.Create(path, thumbnailPath: null, image.ContentType, image.FileSizeBytes,
                                       image.Dimensions, image.Hash);
        var old = command.Image == ShopImage.Logo ? vendor.ChangeLogo(picture) : vendor.ChangeCoverPhoto(picture);
        await unitOfWork.SaveChangesAsync(ct);

        if (old is not null)
            await storage.DeleteAsync(old.Path, StorageContainer.Public, ct);

        return storage.GetPublicUrl(path);
    }
}