using MediatR;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Application.Features.Shipments.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Shipments;

/// <summary>
/// Parcels sent between countries ("Send Food to UK"), in four groups:
/// <list type="number">
///   <item><description><b>Anyone</b> (<c>/api/shipments/routes</c>, <c>/api/shipments/quote</c>,
///   <c>/api/track/{trackingNumber}</c>): where parcels can go, what it costs, and public tracking.</description></item>
///   <item><description><b>Senders</b> (<c>/api/shipments</c>): book, list, open, cancel. Sign-in needed.</description></item>
///   <item><description><b>Test payments (TEMPORARY)</b>: pay a weighed parcel without paying, while
///   <c>Payments:TestPaymentsEnabled</c> is true.</description></item>
///   <item><description><b>Staff</b> (<c>/api/staff/shipments</c>, ShipmentManager or Admin): every parcel,
///   receive at the hub (weigh), move it along its journey.</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>The journey: Booked → (dropped at the hub, weighed) ReceivedAtHub → (sender pays the final price) Paid →
/// InTransit → ArrivedInDestination → OutForDelivery → Delivered. The sender can cancel until it leaves.</para>
/// <para>Parcels are found by their tracking number, never by an Id. Someone else's parcel answers 404.</para>
/// </remarks>
public static class ShipmentEndpoints
{
    /// <summary>Maps the shipment endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder api)
    {
        MapAnyone(api);
        MapSenders(api);
        MapTestPayments(api);
        MapStaff(api);
        return api;
    }

    // ═════════════════════════════ A. Anyone ═════════════════════════════

    private static void MapAnyone(IEndpointRouteBuilder api)
    {
        var shipments = api.MapGroup("/shipments")
                           .WithTags("Shipments");

        shipments.MapGet("/routes", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetShipmentRoutesQuery(), ct)).ToHttp())
            .WithName("GetShipmentRoutes")
            .WithSummary("Where parcels can be sent: routes that are switched on, with drop-off hub and speeds")
            .Produces<IReadOnlyList<ShipmentRouteOptionDto>>();

        shipments.MapPost("/quote", async (GetShipmentQuoteQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetShipmentQuote")
            .WithSummary("Estimated price and transit time for a route, speed and estimated weight")
            .WithDescription("Nothing is saved. The final price is set when the parcel is weighed at the hub.")
            .Produces<ShipmentQuoteDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        api.MapGet("/track/{trackingNumber}", async (string trackingNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new TrackShipmentQuery(trackingNumber), ct)).ToHttp())
            .WithTags("Shipments")
            .WithName("TrackShipment")
            .WithSummary("Public tracking: status and timeline only (no names, phones or addresses)")
            .Produces<PublicTrackingDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ═════════════════════════════ B. Senders ═════════════════════════════

    private static void MapSenders(IEndpointRouteBuilder api)
    {
        var mine = api.MapGroup("/shipments")
                      .WithTags("Shipments")
                      .RequireAuthorization();

        mine.MapPost("/", async (BookShipmentCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("BookShipment")
            .WithSummary("Book a parcel: returns its tracking number, estimated price and where to drop it off")
            .WithDescription("Needs personal details (date of birth + home address) on the account. " +
                             "Pay after the parcel is weighed at the hub.")
            .Produces<BookedShipmentDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        mine.MapGet("/", async ([AsParameters] GetMyShipmentsQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetMyShipments")
            .WithSummary("My parcels, newest first")
            .Produces<PagedResult<ShipmentSummaryDto>>();

        mine.MapGet("/{trackingNumber}", async (string trackingNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyShipmentQuery(trackingNumber), ct)).ToHttp())
            .WithName("GetMyShipment")
            .WithSummary("One of my parcels: recipient, items, weight, price, drop-off and the full timeline")
            .Produces<ShipmentDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        mine.MapPost("/{trackingNumber}/cancel", async (string trackingNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new CancelShipmentCommand(trackingNumber), ct)).ToHttp())
            .WithName("CancelMyShipment")
            .WithSummary("Cancel before the parcel leaves (a paid one is flagged for a refund)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ═════════════════════════════ C. Test payments (TEMPORARY) ═════════════════════════════

    /// <summary>
    /// Maps <c>POST /api/shipments/{trackingNumber}/test-payment</c> only while <c>Payments:TestPaymentsEnabled</c>
    /// is true (the start-up warning is logged by the order endpoints, which use the same setting).
    /// </summary>
    private static void MapTestPayments(IEndpointRouteBuilder api)
    {
        var configuration = api.ServiceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>(AppSettings.Payments.TestPaymentsEnabled)) return;

        api.MapPost("/shipments/{trackingNumber}/test-payment",
                async (string trackingNumber, ISender sender, TimeProvider time, CancellationToken ct) =>
                {
                    // 1. The sender's own parcel (someone else's answers 404), weighed and waiting for payment
                    var shipment = await sender.Send(new GetMyShipmentQuery(trackingNumber), ct);
                    if (shipment.IsFailure) return shipment.ToHttp();

                    // 2. What the payment provider's callback will do: mark it paid
                    var number = shipment.Value.TrackingNumber;
                    var reference = $"TEST-{number}-{time.GetUtcNow():yyyyMMddHHmmss}";
                    var paid = await sender.Send(new MarkShipmentPaidCommand(number, reference), ct);
                    if (paid.IsFailure) return paid.ToHttp();

                    // 3. The parcel as it is now (Paid, with the new timeline step)
                    return (await sender.Send(new GetMyShipmentQuery(number), ct)).ToHttp();
                })
            .RequireAuthorization()
            .WithTags("Shipments")
            .WithName("PayShipmentWithTestPayment")
            .WithSummary("TEST ONLY: pay my weighed parcel without paying (no money moves)")
            .WithDescription("Temporary, until Paystack / Stripe are connected. Exists only while " +
                             "Payments:TestPaymentsEnabled is true.")
            .Produces<ShipmentDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ═════════════════════════════ D. Staff ═════════════════════════════

    private static void MapStaff(IEndpointRouteBuilder api)
    {
        var staff = api.MapGroup("/staff/shipments")
                       .WithTags("Staff: Shipments")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.ShipmentManager, Roles.Admin));

        staff.MapGet("/", async ([AsParameters] GetStaffShipmentsQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetStaffShipments")
            .WithSummary("Every parcel, newest first; filter by status and/or route direction")
            .Produces<PagedResult<StaffShipmentSummaryDto>>()
            .ProducesValidationProblem();

        staff.MapGet("/{trackingNumber}", async (string trackingNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetStaffShipmentQuery(trackingNumber), ct)).ToHttp())
            .WithName("GetStaffShipment")
            .WithSummary("Any parcel's full details and timeline (with staff notes)")
            .Produces<ShipmentDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        staff.MapPost("/{trackingNumber}/receive",
                async (string trackingNumber, ReceiveRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new ReceiveShipmentAtHubCommand(trackingNumber, body.WeighedWeightKg), ct))
                        .ToHttp())
            .WithName("ReceiveShipmentAtHub")
            .WithSummary("Parcel dropped off and weighed at the hub: sets the final price (returned)")
            .Produces<decimal>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        staff.MapPost("/{trackingNumber}/progress",
                async (string trackingNumber, ProgressRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new UpdateShipmentProgressCommand(trackingNumber, body.NewStatus, body.Note),
                                       ct)).ToHttp())
            .WithName("UpdateShipmentProgress")
            .WithSummary("Next step of the journey: InTransit or ArrivedInDestination")
            .WithDescription("Out for delivery: POST …/out-for-delivery with a courier. Delivered: the courier, with " +
                             "a photo (POST /api/deliveries/parcels/{trackingNumber}/delivered).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        staff.MapPost("/{trackingNumber}/out-for-delivery",
                async (string trackingNumber, OutForDeliveryRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new SendOutShipmentCommand(trackingNumber, body.CourierId, body.Note), ct))
                        .ToHttp())
            .WithName("SendOutShipment")
            .WithSummary("Give an arrived parcel to a courier (ArrivedInDestination → OutForDelivery)")
            .WithDescription("courierId from GET /api/staff/couriers?country={destination}. The courier is emailed.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        staff.MapPut("/{trackingNumber}/courier",
                async (string trackingNumber, ChangeCourierRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new ChangeShipmentCourierCommand(trackingNumber, body.CourierId), ct)).ToHttp())
            .WithName("ChangeShipmentCourier")
            .WithSummary("Give a parcel that's out for delivery to another courier")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "received at the hub" (the parcel comes from the URL).</summary>
    public sealed record ReceiveRequest(decimal WeighedWeightKg);

    /// <summary>Body of "next step" (the parcel comes from the URL). <see cref="Note"/> is optional.</summary>
    public sealed record ProgressRequest(ShipmentStatus NewStatus, string? Note);

    /// <summary>Body of "out for delivery": the courier, and an optional timeline note.</summary>
    public sealed record OutForDeliveryRequest(Guid CourierId, string? Note);

    /// <summary>Body of "change courier".</summary>
    public sealed record ChangeCourierRequest(Guid CourierId);
}
