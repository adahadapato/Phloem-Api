using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>Admin creates a route. It starts switched OFF so rates can be added first.</summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination (different from the origin).</param>
/// <param name="DropOffAddress">Origin hub address.</param>
/// <param name="DropOffHours">Origin hub opening hours.</param>
[InvalidatesCache(CacheTags.ShipmentRoutes)]
public sealed record CreateShipmentRouteCommand(Country FromCountry, Country ToCountry,
                                                string DropOffAddress, string DropOffHours) : IRequest<Result>;
