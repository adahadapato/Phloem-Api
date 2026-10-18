using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Same steps as the logo: process → save new file → change cover → save DB → delete OLD file.
/// </summary>
public sealed class ChangeShopCoverCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IImageProcessor imageProcessor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeShopCoverCommand, Result<string>>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<string>> Handle(ChangeShopCoverCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;
        if (vendor.Status == VendorStatus.Blocked) return VendorErrors.Blocked;

        var processed = await imageProcessor.ProcessSimpleImageAsync(command.Content, ct);
        if (processed.IsFailure) return processed.Error;
        await using var image = processed.Value;

        var path = $"vendors/{vendor.Id:N}/cover-{Guid.CreateVersion7():N}.webp";
        await storage.SaveAsync(image.FullSize, path, image.ContentType, StorageContainer.Public, ct);

        var cover = ImageFile.Create(path, thumbnailPath: null, image.ContentType, image.FileSizeBytes,
                                     image.Dimensions, image.Hash);
        var old = vendor.ChangeCoverPhoto(cover);
        await unitOfWork.SaveChangesAsync(ct);

        if (old is not null)
            await storage.DeleteAsync(old.Path, StorageContainer.Public, ct);

        return storage.GetPublicUrl(path);
    }
}