using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>Admin changes a route's drop-off hub address or opening hours.</summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="DropOffAddress">New hub address.</param>
/// <param name="DropOffHours">New opening hours.</param>
[InvalidatesCache(CacheTags.ShipmentRoutes)]
public sealed record UpdateShipmentRouteDropOffCommand(Country FromCountry, Country ToCountry,
                                                       string DropOffAddress, string DropOffHours) : IRequest<Result>;
