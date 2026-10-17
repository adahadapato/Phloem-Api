using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="GetShipmentRoutesQuery"/>.</summary>
public sealed class GetShipmentRoutesQueryHandler(IShipmentRouteRepository routes)
    : IRequestHandler<GetShipmentRoutesQuery, Result<IReadOnlyList<ShipmentRouteOptionDto>>>
{
    /// <summary>Active routes that offer at least one speed, ordered by origin then destination.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>The routes (possibly empty).</returns>
    public async Task<Result<IReadOnlyList<ShipmentRouteOptionDto>>> Handle(GetShipmentRoutesQuery query, CancellationToken ct)
    {
        var active = await routes.GetActiveAsync(ct);

        IReadOnlyList<ShipmentRouteOptionDto> items = active
            .Where(r => r.Rates.Count > 0)
            .OrderBy(r => r.FromCountry).ThenBy(r => r.ToCountry)
            .Select(ShipmentRouteOptionDto.From)
            .ToList();

        return Result<IReadOnlyList<ShipmentRouteOptionDto>>.Success(items);
    }
}
