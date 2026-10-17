using MediatR;
using Phloem.Application.Features.ShipmentRoutes.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Queries;

/// <summary>Admin → Shipping routes: every route, on or off. Not cached (admins need live data).</summary>
public sealed record GetAdminShipmentRoutesQuery : IRequest<Result<IReadOnlyList<AdminShipmentRouteDto>>>;
