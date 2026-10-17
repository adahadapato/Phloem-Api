using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

public sealed class CompleteTwoFactorLoginCommandHandler(
    IAuthService auth,
    IUserProfileRepository profiles,
    IAccountService accounts)
    : IRequestHandler<CompleteTwoFactorLoginCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(CompleteTwoFactorLoginCommand command, CancellationToken ct)
    {
        var result = await auth.CompleteTwoFactorLoginAsync(
            command.ChallengeToken,
            command.Code.Replace(" ", string.Empty),   // "123 456" → "123456"
            command.IsRecoveryCode,
            ct);
        if (result.IsFailure) return result.Error;

        var tokens = result.Value;

        var profile = await profiles.GetByIdAsync(tokens.UserId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var contact = await accounts.GetContactAsync(tokens.UserId, ct);
        if (contact.IsFailure) return contact.Error;

        return AuthTokensDto.SignedIn(tokens, profile.GetFullName(), contact.Value.Email);
    }
}