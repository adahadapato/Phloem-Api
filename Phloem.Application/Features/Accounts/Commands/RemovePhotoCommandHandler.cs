using MediatR;
using Phloem.Domain.Errors;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Clears the profile photo. Order: update profile → save DB → delete the file
/// (if the save fails, the photo is still there and nothing is lost).
/// </summary>
public sealed class RemovePhotoCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemovePhotoCommand, Result>
{
    public async Task<Result> Handle(RemovePhotoCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var oldPath = profile.PhotoUrl;
        if (oldPath is null) return Result.Success();   // nothing to remove

        var result = profile.ChangePhoto(null);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(ct);
        await storage.DeleteAsync(oldPath, StorageContainer.Public, ct);

        return Result.Success();
    }
}