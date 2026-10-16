using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Simple processing (no enhancement), public container.
/// Order: process → save new file → change logo → save DB → delete OLD file
/// (logos aren't copied into orders, so the old file can go).
/// </summary>
public sealed class ChangeShopLogoCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IImageProcessor imageProcessor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeShopLogoCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChangeShopLogoCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;
        if (vendor.Status == Domain.Enums.VendorStatus.Blocked) return VendorErrors.Blocked;

        var processed = await imageProcessor.ProcessSimpleImageAsync(command.Content, ct);
        if (processed.IsFailure) return processed.Error;

        await using var image = processed.Value;

        var path = $"vendors/{vendor.Id:N}/logo-{Guid.CreateVersion7():N}.webp";
        await storage.SaveAsync(image.FullSize, path, image.ContentType, StorageContainer.Public, ct);

        var logo = ImageFile.Create(path, thumbnailPath: null, image.ContentType, image.FileSizeBytes,
                                    image.Dimensions, image.Hash);

        var old = vendor.ChangeLogo(logo);
        await unitOfWork.SaveChangesAsync(ct);

        if (old is not null)
            await storage.DeleteAsync(old.Path, StorageContainer.Public, ct);

        return storage.GetPublicUrl(path);
    }
}