using MediatR;
using Phloem.Application.Features.Commissions.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Commissions.Queries;

/// <summary>Admin → Commissions page: EVERY country with its rate (0% where none is set). Not cached.</summary>
public sealed record GetAdminCommissionsQuery : IRequest<Result<IReadOnlyList<CommissionDto>>>;