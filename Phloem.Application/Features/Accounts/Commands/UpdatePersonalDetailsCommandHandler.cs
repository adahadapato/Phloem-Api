using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Saves the signed-in user's date of birth and home address.</summary>
public sealed class UpdatePersonalDetailsCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<UpdatePersonalDetailsCommand, Result>
{
    /// <summary>Loads the profile, lets the Domain check the date, saves.</summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(UpdatePersonalDetailsCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var updated = profile.UpdatePersonalDetails(command.DateOfBirth, command.HomeAddress.ToResidentialAddress(),
                                                    today);
        if (updated.IsFailure) return updated;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}