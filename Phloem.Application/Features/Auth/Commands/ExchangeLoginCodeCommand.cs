using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>
/// Last step of "Continue with Google": the app swaps the one-time code from <c>phloem://auth?code=…</c> for a
/// session. Signed in, or RequiresTwoFactor = true with a challenge token (then <see cref="CompleteTwoFactorLoginCommand"/>),
/// exactly like <see cref="LoginCommand"/>.
/// </summary>
/// <param name="Code">The one-time code from the link. Works once, for about a minute.</param>
public sealed record ExchangeLoginCodeCommand(string Code) : IRequest<Result<AuthTokensDto>>;
