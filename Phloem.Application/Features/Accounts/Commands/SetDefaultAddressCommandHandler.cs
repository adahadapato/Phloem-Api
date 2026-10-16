using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

public sealed class SetDefaultAddressCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetDefaultAddressCommand, Result>
{
    public async Task<Result> Handle(SetDefaultAddressCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var result = profile.SetDefaultAddress(command.AddressId);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}