using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

public sealed class LoginCommandHandler(
    IAuthService auth,
    IUserProfileRepository profiles,
    IAccountService accounts)
    : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(LoginCommand command, CancellationToken ct)
    {
        var login = await auth.LoginAsync(command.Email.Trim(), command.Password, ct);
        if (login.IsFailure) return login.Error;

        // Password OK, but a 2FA code is needed first
        if (login.Value.RequiresTwoFactor)
            return AuthTokensDto.TwoFactorRequired(login.Value.ChallengeToken!);

        var tokens = login.Value.Tokens
            ?? throw new InvalidOperationException("Login succeeded without tokens or a 2FA challenge.");

        var profile = await profiles.GetByIdAsync(tokens.UserId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var contact = await accounts.GetContactAsync(tokens.UserId, ct);
        if (contact.IsFailure) return contact.Error;

        return AuthTokensDto.SignedIn(tokens, profile.FullName, contact.Value.Email);
    }
}