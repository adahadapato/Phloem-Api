using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>
/// Handles <see cref="ExchangeLoginCodeCommand"/>: uses the code up, then signs the account in with the same checks as
/// a password login (lockout, deactivated, suspended, 2FA), and returns the same <see cref="AuthTokensDto"/>.
/// </summary>
/// <remarks>The session starts HERE, not when Google answered, so someone suspended in the meantime is refused and
/// tokens never sit in memory waiting.</remarks>
/// <param name="codes">The one-time codes.</param>
/// <param name="auth">Signs the account in.</param>
/// <param name="profiles">The person's name for the response.</param>
/// <param name="accounts">The person's email for the response.</param>
public sealed class ExchangeLoginCodeCommandHandler(
    ILoginCodeStore codes,
    IAuthService auth,
    IUserProfileRepository profiles,
    IAccountService accounts)
    : IRequestHandler<ExchangeLoginCodeCommand, Result<AuthTokensDto>>
{
    /// <summary>Swaps the code for a session.</summary>
    /// <param name="command">The one-time code.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>Signed in or waiting for a 2FA code; or <c>AuthErrors.InvalidLoginCode</c> (unknown, used or expired)
    /// or a sign-in error (locked out, deactivated, suspended).</returns>
    public async Task<Result<AuthTokensDto>> Handle(ExchangeLoginCodeCommand command, CancellationToken ct)
    {
        if (!codes.TryRedeem(command.Code, out var userId)) return AuthErrors.InvalidLoginCode;

        var login = await auth.SignInExternalAsync(userId, ct);
        if (login.IsFailure) return login.Error;

        // Google proved who they are, but their 2FA code is still needed
        if (login.Value.RequiresTwoFactor)
            return AuthTokensDto.TwoFactorRequired(login.Value.ChallengeToken!);

        var tokens = login.Value.Tokens
            ?? throw new InvalidOperationException("Sign-in succeeded without tokens or a 2FA challenge.");

        var profile = await profiles.GetByIdAsync(tokens.UserId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var contact = await accounts.GetContactAsync(tokens.UserId, ct);
        if (contact.IsFailure) return contact.Error;

        return AuthTokensDto.SignedIn(tokens, profile.GetFullName(), contact.Value.Email);
    }
}
