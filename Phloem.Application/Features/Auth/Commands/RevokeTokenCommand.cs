using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>Account → Log out: the refresh token can never be used again.</summary>
public sealed record RevokeTokenCommand(string RefreshToken) : IRequest<Result>;