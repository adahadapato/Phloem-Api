using MediatR;
using Phloem.Application.Features.ShipmentRoutes.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Queries;

/// <summary>Handles <see cref="GetAdminShipmentRoutesQuery"/>.</summary>
public sealed class GetAdminShipmentRoutesQueryHandler(IShipmentRouteRepository routes)
    : IRequestHandler<GetAdminShipmentRoutesQuery, Result<IReadOnlyList<AdminShipmentRouteDto>>>
{
    /// <summary>All routes, ordered by origin then destination.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>Every route (possibly empty).</returns>
    public async Task<Result<IReadOnlyList<AdminShipmentRouteDto>>> Handle(GetAdminShipmentRoutesQuery query, CancellationToken ct)
    {
        var all = await routes.GetAllAsync(ct);

        IReadOnlyList<AdminShipmentRouteDto> items = all
            .OrderBy(r => r.FromCountry).ThenBy(r => r.ToCountry)
            .Select(AdminShipmentRouteDto.From)
            .ToList();

        return Result<IReadOnlyList<AdminShipmentRouteDto>>.Success(items);
    }
}
