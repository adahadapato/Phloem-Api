using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Accounts.Commands;

public sealed class UpdateAddressCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAddressCommand, Result>
{
    public async Task<Result> Handle(UpdateAddressCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var result = profile.UpdateAddress(command.AddressId, command.Label, command.FullAddress, command.Country,
                                           command.RecipientName, PhoneNumber.CreateOptional(command.PhoneNumber));
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}