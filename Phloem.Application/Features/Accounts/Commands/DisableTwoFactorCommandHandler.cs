using MediatR;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Handles <see cref="DisableTwoFactorCommand"/>.</summary>
public sealed class DisableTwoFactorCommandHandler(ICurrentUser currentUser, IAccountService accounts)
    : IRequestHandler<DisableTwoFactorCommand, Result>
{
    /// <summary>
    /// Refuses admins, checks the password (so a stolen, unlocked phone can't switch it off), then disables.
    /// </summary>
    /// <returns>Success; or <see cref="AuthErrors.TwoFactorRequired"/>, a wrong-password error, or an Identity error.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(DisableTwoFactorCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        if (currentUser.IsInRole(Roles.Admin)) return AuthErrors.TwoFactorRequired;

        var password = await accounts.VerifyPasswordAsync(userId, command.Password, ct);
        if (password.IsFailure) return password;

        return await accounts.DisableTwoFactorAsync(userId, ct);
    }
}