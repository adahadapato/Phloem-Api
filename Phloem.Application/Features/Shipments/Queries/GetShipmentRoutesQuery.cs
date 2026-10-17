using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>
/// "Send a parcel" step 1: the switched-on routes with their hubs, speeds, prices and transit days.
/// Public and cached; cleared by every admin route command.
/// </summary>
[Cacheable(CacheTags.ShipmentRoutes, Minutes = 60)]
public sealed record GetShipmentRoutesQuery : IRequest<Result<IReadOnlyList<ShipmentRouteOptionDto>>>;
