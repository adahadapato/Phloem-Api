using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Name + country live on UserProfile; phone lives on the login account (Identity).</summary>
public sealed class UpdateProfileCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IAccountService accounts,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        // 1. Business rules first (e.g. deleted account): nothing is changed if this fails
        var updated = profile.UpdateProfile(command.FirstName, command.LastName, command.Country);
        if (updated.IsFailure) return updated;

        // 2. Phone on the login account, normalised to "+digits" (null = remove)
        var phone = PhoneNumber.CreateOptional(command.PhoneNumber)?.Value;
        var phoneResult = await accounts.UpdatePhoneNumberAsync(userId, phone, ct);
        if (phoneResult.IsFailure) return phoneResult;

        // 3. Save the profile
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}