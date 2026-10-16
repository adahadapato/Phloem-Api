using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Profile photo: simple processing (no enhancement), PUBLIC container.
/// Order: process → save new file → update profile → save DB → delete OLD file.
/// </summary>
public sealed class ChangePhotoCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IImageProcessor imageProcessor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangePhotoCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChangePhotoCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        // 1. Process (rejects non-images, strips GPS, shrinks)
        var processed = await imageProcessor.ProcessSimpleImageAsync(command.Content, ct);
        if (processed.IsFailure) return processed.Error;

        await using var image = processed.Value;

        // 2. Save under a NEW name (so phones don't show a cached old photo)
        var newPath = $"profiles/{userId:N}/{Guid.CreateVersion7():N}.webp";
        await storage.SaveAsync(image.FullSize, newPath, image.ContentType, StorageContainer.Public, ct);

        // 3. Domain rule (e.g. deleted account): if it fails, remove the file we just saved
        var oldPath = profile.PhotoUrl;
        var changed = profile.ChangePhoto(newPath);
        if (changed.IsFailure)
        {
            await storage.DeleteAsync(newPath, StorageContainer.Public, ct);
            return changed.Error;
        }

        // 4. Save, THEN delete the old file (if the save fails, the old photo is still there)
        await unitOfWork.SaveChangesAsync(ct);

        if (oldPath is not null)
            await storage.DeleteAsync(oldPath, StorageContainer.Public, ct);

        return storage.GetPublicUrl(newPath);
    }
}