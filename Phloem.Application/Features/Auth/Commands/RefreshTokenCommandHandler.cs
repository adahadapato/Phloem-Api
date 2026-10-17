using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>
/// Also returns name, email and the CURRENT roles, so if an admin approved the shop
/// (Seller role added) the app picks it up on the next refresh.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IAuthService auth,
    IUserProfileRepository profiles,
    IAccountService accounts)
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await auth.RefreshTokenAsync(command.RefreshToken, ct);
        if (result.IsFailure) return result.Error;

        var tokens = result.Value;

        var profile = await profiles.GetByIdAsync(tokens.UserId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var contact = await accounts.GetContactAsync(tokens.UserId, ct);
        if (contact.IsFailure) return contact.Error;

        return AuthTokensDto.SignedIn(tokens, profile.GetFullName(), contact.Value.Email);
    }
}