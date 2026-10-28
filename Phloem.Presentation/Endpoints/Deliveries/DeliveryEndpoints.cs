using MediatR;
using Microsoft.AspNetCore.Mvc;
using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Application.Features.Deliveries.Queries;
using Phloem.Application.Features.Orders.Commands;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Deliveries;

/// <summary>
/// Deliveries:
/// <list type="bullet">
///   <item><description><b>Couriers</b> (<c>/api/deliveries</c>): "My deliveries" and "Delivered" with a photo, for food
///   orders and parcels. Open to any signed-in person, because a seller can deliver their own orders: each request
///   only ever sees what was given to the caller.</description></item>
///   <item><description><b>Parcel staff</b> (<c>/api/staff/couriers</c>): the couriers in a country, for "Out for
///   delivery".</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>The seller's own courier list and "Send out" / "Change courier" are with the shop orders
/// (<c>/api/shops/mine/…</c>); parcel "Out for delivery" / "Change courier" are with the staff shipments.</para>
/// <para>"Delivered" is a multipart form: <c>photo</c> (file, required), <c>handover</c> (InPerson, FrontDoor,
/// Neighbour, SafePlace or Reception), <c>receivedBy</c> (required for Neighbour) and <c>note</c>.</para>
/// </remarks>
public static class DeliveryEndpoints
{
    /// <summary>Maps the delivery endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder api)
    {
        MapCouriers(api);
        MapStaff(api);
        return api;
    }

    // ═════════════════════════════ A. Couriers ═════════════════════════════

    private static void MapCouriers(IEndpointRouteBuilder api)
    {
        var deliveries = api.MapGroup("/deliveries")
                            .WithTags("Deliveries")
                            .RequireAuthorization();

        deliveries.MapGet("/", async (bool? done, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyDeliveriesQuery(done ?? false), ct)).ToHttp())
            .WithName("GetMyDeliveries")
            .WithSummary("My deliveries: still to deliver (oldest first), or done=true for the last 7 days")
            .Produces<IReadOnlyList<DeliveryJobDto>>();

        deliveries.MapPost("/orders/{orderNumber}/delivered",
                async (string orderNumber, IFormFile? photo, [FromForm] HandoverMethod? handover,
                       [FromForm] string? receivedBy, [FromForm] string? note, ISender sender, CancellationToken ct) =>
                {
                    // Missing photo / handover reach the validator, which explains them field by field
                    await using var content = photo?.OpenReadStream() ?? Stream.Null;
                    var command = new MarkOrderDeliveredCommand(orderNumber, content, photo?.Length ?? 0,
                                                                handover ?? default, receivedBy, note);
                    return (await sender.Send(command, ct)).ToHttp();
                })
            .WithName("MarkOrderDelivered")
            .WithSummary("Delivered, with a photo: the shop is paid and the customer is emailed")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .DisableAntiforgery();   // JWT API: no cookies, so no cross-site form risk

        deliveries.MapPost("/parcels/{trackingNumber}/delivered",
                async (string trackingNumber, IFormFile? photo, [FromForm] HandoverMethod? handover,
                       [FromForm] string? receivedBy, [FromForm] string? note, ISender sender, CancellationToken ct) =>
                {
                    // Missing photo / handover reach the validator, which explains them field by field
                    await using var content = photo?.OpenReadStream() ?? Stream.Null;
                    var command = new MarkShipmentDeliveredCommand(trackingNumber, content, photo?.Length ?? 0,
                                                                   handover ?? default, receivedBy, note);
                    return (await sender.Send(command, ct)).ToHttp();
                })
            .WithName("MarkParcelDelivered")
            .WithSummary("Delivered, with a photo: the timeline is updated and the sender is emailed")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .DisableAntiforgery();
    }

    // ═════════════════════════════ B. Parcel staff ═════════════════════════════

    private static void MapStaff(IEndpointRouteBuilder api)
    {
        api.MapGet("/staff/couriers", async (Country country, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetCouriersQuery(country), ct)).ToHttp())
            .RequireAuthorization(policy => policy.RequireRole(Roles.ShipmentManager, Roles.Admin))
            .WithTags("Staff: Shipments")
            .WithName("GetCouriers")
            .WithSummary("Couriers who deliver in a country (for \"Out for delivery\"), by name")
            .Produces<IReadOnlyList<CourierOptionDto>>()
            .ProducesValidationProblem();
    }
}
