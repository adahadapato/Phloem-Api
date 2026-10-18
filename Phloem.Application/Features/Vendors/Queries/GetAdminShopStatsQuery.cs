using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>Admin dashboard tiles: how many shops in each status.</summary>
public sealed record GetAdminShopStatsQuery : IRequest<Result<AdminShopStatsDto>>;
