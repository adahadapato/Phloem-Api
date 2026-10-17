using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Handles <see cref="RegenerateRecoveryCodesCommand"/>.</summary>
public sealed class RegenerateRecoveryCodesCommandHandler(ICurrentUser currentUser, IAccountService accounts)
    : IRequestHandler<RegenerateRecoveryCodesCommand, Result<IReadOnlyCollection<string>>>
{
    /// <summary>Checks the password, then asks Identity for a fresh set.</summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<IReadOnlyCollection<string>>> Handle(RegenerateRecoveryCodesCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var password = await accounts.VerifyPasswordAsync(userId, command.Password, ct);
        if (password.IsFailure) return password.Error;

        return await accounts.GenerateRecoveryCodesAsync(userId, ct);
    }
}