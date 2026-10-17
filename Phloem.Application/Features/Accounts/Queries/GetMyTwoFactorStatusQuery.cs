using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

/// <summary>Account → Security: the signed-in user's two-factor status. Never cached.</summary>
public sealed record GetMyTwoFactorStatusQuery : IRequest<Result<TwoFactorStatusDto>>;