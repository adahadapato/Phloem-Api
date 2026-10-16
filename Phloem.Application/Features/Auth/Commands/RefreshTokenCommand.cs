using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>Silent: swaps the refresh token for a new pair to stay signed in.</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokensDto>>;