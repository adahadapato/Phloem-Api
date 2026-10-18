using MediatR;
using Phloem.Application.Features.ShipmentRoutes.Commands;
using Phloem.Application.Features.ShipmentRoutes.Dtos;
using Phloem.Application.Features.ShipmentRoutes.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.ShipmentRoutes;

/// <summary>
/// Shipping routes between countries (e.g. UnitedKingdom → Nigeria), each with a drop-off hub and a rate per speed.
/// <list type="number">
///   <item><description><b>Admin</b> (<c>/api/admin/shipment-routes</c>): create routes, set rates, switch routes on
///   and off. Admin role only.</description></item>
/// </list>
/// </summary>
/// <remarks>
/// A route is identified by its two countries, so URLs read <c>/{fromCountry}/{toCountry}</c>. New routes start
/// switched OFF, so rates can be added before customers see them. The buyer side (pick a route, get a quote) comes
/// with the shipments batch.
/// </remarks>
public static class ShipmentRouteEndpoints
{
    /// <summary>Maps the shipment route endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapShipmentRouteEndpoints(this IEndpointRouteBuilder api)
    {
        MapAdmin(api);
        return api;
    }

    // ═════════════════════════════ A. Admin ═════════════════════════════

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/shipment-routes")
                       .WithTags("Admin: Shipping routes")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        // ───────────────────────────── 1. Routes ─────────────────────────────

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminShipmentRoutesQuery(), ct)).ToHttp())
            .WithName("GetAdminShipmentRoutes")
            .WithSummary("Every route with its drop-off hub, rates and whether it's switched on")
            .Produces<IReadOnlyList<AdminShipmentRouteDto>>();

        admin.MapPost("/", async (CreateShipmentRouteCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("CreateShipmentRoute")
            .WithSummary("Create a route (starts switched off: add rates, then switch it on)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPut("/{fromCountry}/{toCountry}/drop-off",
                async (Country fromCountry, Country toCountry, DropOffRequest body, ISender sender,
                       CancellationToken ct) =>
                    (await sender.Send(new UpdateShipmentRouteDropOffCommand(fromCountry, toCountry,
                                                                             body.DropOffAddress, body.DropOffHours),
                                       ct)).ToHttp())
            .WithName("UpdateShipmentRouteDropOff")
            .WithSummary("Change where and when customers drop parcels off for this route")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPut("/{fromCountry}/{toCountry}/active",
                async (Country fromCountry, Country toCountry, ActiveRequest body, ISender sender,
                       CancellationToken ct) =>
                    (await sender.Send(new SetShipmentRouteActiveCommand(fromCountry, toCountry, body.IsActive), ct))
                        .ToHttp())
            .WithName("SetShipmentRouteActive")
            .WithSummary("Switch a route on (needs at least one rate) or off")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ───────────────────────────── 2. Rates ─────────────────────────────

        admin.MapPut("/{fromCountry}/{toCountry}/rates/{speed}",
                async (Country fromCountry, Country toCountry, DeliverySpeed speed, ShipmentRateRequest body,
                       ISender sender, CancellationToken ct) =>
                    (await sender.Send(new SetShipmentRateCommand(fromCountry, toCountry, speed, body.BaseCharge,
                                                                  body.PricePerKg, body.MinTransitDays,
                                                                  body.MaxTransitDays), ct)).ToHttp())
            .WithName("SetShipmentRate")
            .WithSummary("Set one speed's price and transit time (only parcels weighed from now on)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapDelete("/{fromCountry}/{toCountry}/rates/{speed}",
                async (Country fromCountry, Country toCountry, DeliverySpeed speed, ISender sender,
                       CancellationToken ct) =>
                    (await sender.Send(new RemoveShipmentRateCommand(fromCountry, toCountry, speed), ct)).ToHttp())
            .WithName("RemoveShipmentRate")
            .WithSummary("Stop offering a speed on this route")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "change the drop-off" (the route comes from the URL).</summary>
    public sealed record DropOffRequest(string DropOffAddress, string DropOffHours);

    /// <summary>Body of "switch a route on or off".</summary>
    public sealed record ActiveRequest(bool IsActive);

    /// <summary>Body of "set a rate" (route and speed come from the URL). Amounts are in the origin's currency.</summary>
    public sealed record ShipmentRateRequest(decimal BaseCharge, decimal PricePerKg, int MinTransitDays,
                                             int MaxTransitDays);
}
