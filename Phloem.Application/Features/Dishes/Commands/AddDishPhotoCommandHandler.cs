using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Dishes.Commands;

/// <summary>
/// Order: check ownership → process → save both files → Domain rules → save DB.
/// If the Domain says no (too many / duplicate), the files we just saved are removed.
/// </summary>
public sealed class AddDishPhotoCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDishRepository dishes,
    IImageProcessor imageProcessor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddDishPhotoCommand, Result<DishPhotoDto>>
{
    public async Task<Result<DishPhotoDto>> Handle(AddDishPhotoCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var dish = await dishes.GetByIdAsync(command.DishId, ct);
        if (dish is null || dish.VendorId != vendor.Id) return DishErrors.NotFound;

        // 1. Strict pipeline (too small / blurry / not an image → ImageErrors)
        var processed = await imageProcessor.ProcessDishPhotoAsync(command.Content, ct);
        if (processed.IsFailure) return processed.Error;

        await using var image = processed.Value;
        var thumbnail = image.Thumbnail
            ?? throw new InvalidOperationException("The dish-photo pipeline must produce a thumbnail.");

        // 2. Save both files (public container)
        var name = Guid.CreateVersion7().ToString("N");
        var fullPath = $"dishes/{dish.Id:N}/{name}.webp";
        var thumbPath = $"dishes/{dish.Id:N}/{name}_thumb.webp";

        await storage.SaveAsync(image.FullSize, fullPath, image.ContentType, StorageContainer.Public, ct);
        await storage.SaveAsync(thumbnail, thumbPath, image.ContentType, StorageContainer.Public, ct);

        // 3. Domain rules (max 5, no duplicates)
        var file = ImageFile.Create(fullPath, thumbPath, image.ContentType, image.FileSizeBytes,
                                    image.Dimensions, image.Hash);

        var added = dish.AddPhoto(file);
        if (added.IsFailure)
        {
            await storage.DeleteAsync(fullPath, StorageContainer.Public, ct);
            await storage.DeleteAsync(thumbPath, StorageContainer.Public, ct);
            return added.Error;
        }

        // 4. Save
        await unitOfWork.SaveChangesAsync(ct);
        return DishPhotoDto.From(added.Value, storage);
    }
}