using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>Login page. Signed in, or RequiresTwoFactor = true with a challenge token.</summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthTokensDto>>;