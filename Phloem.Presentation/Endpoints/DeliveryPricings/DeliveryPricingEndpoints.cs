using MediatR;
using Phloem.Application.Features.DeliveryPricings.Commands;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Application.Features.DeliveryPricings.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.DeliveryPricings;

/// <summary>
/// Delivery pricing, per country: the range a shop's own delivery fee must be inside, and the extra charge for each
/// delivery window (ASAP, this evening, tomorrow).
/// <list type="number">
///   <item><description><b>Buyers</b> (<c>/api/delivery-windows</c>): the "Delivery time" picker. Open to anyone:
///   it's only prices, and buyers may look before signing in.</description></item>
///   <item><description><b>Admin</b> (<c>/api/admin/delivery-pricing</c>): set the ranges and window charges.
///   Admin role only.</description></item>
/// </list>
/// </summary>
/// <remarks>A seller's own fee is set in <c>ShopEndpoints</c> (<c>/api/shops/mine/delivery-fee</c>).</remarks>
public static class DeliveryPricingEndpoints
{
    /// <summary>Maps the delivery pricing endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapDeliveryPricingEndpoints(this IEndpointRouteBuilder api)
    {
        MapBuyers(api);
        MapAdmin(api);
        return api;
    }

    // ═════════════════════════════ A. Buyers ═════════════════════════════

    private static void MapBuyers(IEndpointRouteBuilder api)
    {
        var windows = api.MapGroup("/delivery-windows")
                         .WithTags("Delivery");

        windows.MapGet("/", async ([AsParameters] GetDeliveryWindowsQuery query, ISender sender,
                                   CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetDeliveryWindows")
            .WithSummary("Delivery times offered in a country and what each adds to the order")
            .Produces<IReadOnlyList<DeliveryWindowOptionDto>>()
            .ProducesValidationProblem();
    }

    // ═════════════════════════════ B. Admin ═════════════════════════════

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/delivery-pricing")
                       .WithTags("Admin: Delivery pricing")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        // ───────────────────────────── 1. View ─────────────────────────────

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminDeliveryPricingsQuery(), ct)).ToHttp())
            .WithName("GetAdminDeliveryPricing")
            .WithSummary("Every country's shop fee range and delivery window charges")
            .Produces<IReadOnlyList<DeliveryPricingDto>>();

        // ───────────────────────────── 2. Shop fee range ─────────────────────────────

        admin.MapPut("/{country}", async (Country country, ShopFeeRangeRequest body, ISender sender,
                                          CancellationToken ct) =>
                (await sender.Send(new SetDeliveryPricingCommand(country, body.MinShopFee, body.MaxShopFee), ct))
                    .ToHttp())
            .WithName("SetDeliveryPricing")
            .WithSummary("Set a country's shop delivery-fee range; returns how many shops had their fee moved")
            .WithDescription("Shops whose fee is now outside the range are moved to the nearest limit and emailed.")
            .Produces<int>()
            .ProducesValidationProblem();

        // ───────────────────────────── 3. Delivery windows ─────────────────────────────

        admin.MapPut("/{country}/windows/{window}", async (Country country, DeliveryWindow window,
                                                           WindowChargeRequest body, ISender sender,
                                                           CancellationToken ct) =>
                (await sender.Send(new SetDeliveryWindowChargeCommand(country, window, body.ExtraCharge), ct))
                    .ToHttp())
            .WithName("SetDeliveryWindowCharge")
            .WithSummary("Offer a delivery window in a country, or change what it adds (0 = free)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapDelete("/{country}/windows/{window}", async (Country country, DeliveryWindow window,
                                                              ISender sender, CancellationToken ct) =>
                (await sender.Send(new RemoveDeliveryWindowCommand(country, window), ct)).ToHttp())
            .WithName("RemoveDeliveryWindow")
            .WithSummary("Stop offering a delivery window in a country")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "set the shop fee range" (the country comes from the URL). Both ends are allowed.</summary>
    public sealed record ShopFeeRangeRequest(decimal MinShopFee, decimal MaxShopFee);

    /// <summary>Body of "set a window charge": what the window adds to an order.</summary>
    public sealed record WindowChargeRequest(decimal ExtraCharge);
}
