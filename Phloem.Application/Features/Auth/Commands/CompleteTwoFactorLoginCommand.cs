using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>Step 2 of a 2FA login: authenticator code, or a recovery code when IsRecoveryCode is true.</summary>
public sealed record CompleteTwoFactorLoginCommand(string ChallengeToken, string Code, bool IsRecoveryCode)
    : IRequest<Result<AuthTokensDto>>;