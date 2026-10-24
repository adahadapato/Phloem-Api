using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Application.Features.Orders.Commands;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Application.Features.Orders.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Orders;

/// <summary>
/// Every order endpoint, in two groups:
/// <list type="number">
///   <item><description><b>Buyers</b> (<c>/api/orders</c>): check the basket, place orders, pay, follow and cancel
///   them. Checking the basket works for guests; everything else needs sign-in.</description></item>
///   <item><description><b>My shop</b> (<c>/api/shops/mine/orders</c>): the seller's incoming orders: accept, send out,
///   mark delivered. "mine" comes from the token, never the URL.</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>Orders are found by their <b>order number</b> (e.g. "PH-261018-482193"), the number people see and quote,
/// never by an Id. Someone else's order answers 404.</para>
/// <para><b>Test payments (TEMPORARY):</b> while <c>Payments:TestPaymentsEnabled</c> is true, buyers can pay their own
/// order with a pretend payment so the owners can try the whole flow before Paystack / Stripe are connected. When it's
/// false the endpoint doesn't exist at all (404). Turn it off before real customers use the app.</para>
/// </remarks>
public static class OrderEndpoints
{
    /// <summary>Maps the order endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder api)
    {
        MapBuyers(api);
        MapTestPayments(api);
        MapMyShop(api);
        return api;
    }

    // ═════════════════════════════ A. Buyers ═════════════════════════════

    private static void MapBuyers(IEndpointRouteBuilder api)
    {
        // ───────────────────────────── 1. Basket (guests too) ─────────────────────────────

        var basket = api.MapGroup("/orders/basket")
                        .WithTags("Orders");

        basket.MapPost("/check", async (CheckBasketQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("CheckBasket")
            .WithSummary("Checkout page: today's prices, out-of-stock dishes and delivery fees, split per shop")
            .WithDescription("Nothing is saved. Works without signing in. The app sends only dish Ids and quantities; " +
                             "every price comes from the server.")
            .Produces<BasketCheckDto>()
            .ProducesValidationProblem();

        // ───────────────────────────── 2. My orders ─────────────────────────────

        var orders = api.MapGroup("/orders")
                        .WithTags("Orders")
                        .RequireAuthorization();

        orders.MapPost("/", async (PlaceOrderCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("PlaceOrder")
            .WithSummary("Place order: one UNPAID order per shop in the basket; returns their order numbers")
            .WithDescription("Use one of your saved addresses (GET /api/me), in the shop's country. " +
                             "Then pay each order with POST /api/orders/{orderNumber}/payment.")
            .Produces<IReadOnlyList<PlacedOrderDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        orders.MapGet("/", async ([AsParameters] GetMyOrdersQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetMyOrders")
            .WithSummary("My orders, newest first; optionally one tab (show), placed since a day (from) and a search")
            .WithDescription("show = All | WaitingForPayment | InProgress | Completed | Cancelled. " +
                             "from = yyyy-MM-dd (e.g. three months ago). search = order number, shop or dish name.")
            .Produces<PagedResult<OrderSummaryDto>>()
            .ProducesValidationProblem();

        orders.MapGet("/recent-dishes", async ([AsParameters] GetRecentlyOrderedDishesQuery query, ISender sender,
                                               CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetRecentlyOrderedDishes")
            .WithSummary("\"Buy it again\": dishes from my paid orders still on sale, most recent first (dish cards)")
            .Produces<IReadOnlyList<DishCardDto>>()
            .ProducesValidationProblem();

        orders.MapGet("/{orderNumber}", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetOrderQuery(orderNumber), ct)).ToHttp())
            .WithName("GetMyOrder")
            .WithSummary("One of my orders, with its lines, fees and status")
            .Produces<OrderDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        orders.MapPost("/{orderNumber}/payment", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new PrepareOrderPaymentCommand(orderNumber), ct)).ToHttp())
            .WithName("PrepareOrderPayment")
            .WithSummary("\"Pay\": refreshes prices to today's, drops out-of-stock dishes, returns the amount to pay")
            .Produces<OrderPaymentDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        orders.MapPost("/{orderNumber}/cancel", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new CancelOrderCommand(orderNumber), ct)).ToHttp())
            .WithName("CancelMyOrder")
            .WithSummary("Cancel: unpaid any time; paid within 30 minutes of payment and before the shop starts")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        orders.MapGet("/{orderNumber}/buy-again", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetBuyAgainQuery(orderNumber), ct)).ToHttp())
            .WithName("BuyAgain")
            .WithSummary("\"Buy again\": the order's dishes at today's prices, ready to put back in the basket")
            .Produces<BuyAgainDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ═════════════════════════════ B. Test payments (TEMPORARY) ═════════════════════════════

    /// <summary>
    /// Maps <c>POST /api/orders/{orderNumber}/test-payment</c> only while <c>Payments:TestPaymentsEnabled</c> is
    /// true, and logs a warning at start-up so it can't be forgotten.
    /// </summary>
    private static void MapTestPayments(IEndpointRouteBuilder api)
    {
        var configuration = api.ServiceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>(AppSettings.Payments.TestPaymentsEnabled)) return;

        api.ServiceProvider.GetRequiredService<ILoggerFactory>()
           .CreateLogger(typeof(OrderEndpoints))
           .LogWarning("TEST PAYMENTS ARE ON: buyers can pay orders without paying. " +
                       "Set {Setting} to false before real customers use the app.",
                       AppSettings.Payments.TestPaymentsEnabled);

        api.MapPost("/orders/{orderNumber}/test-payment", async (string orderNumber, ISender sender, TimeProvider time,
                                                               CancellationToken ct) =>
            {
                // 1. Exactly what "Pay" does: owner check, today's prices, out-of-stock dishes dropped
                var prepared = await sender.Send(new PrepareOrderPaymentCommand(orderNumber), ct);
                if (prepared.IsFailure) return prepared.ToHttp();

                // 2. What the payment provider's callback will do: mark it paid
                var reference = $"TEST-{prepared.Value.OrderNumber}-{time.GetUtcNow():yyyyMMddHHmmss}";
                var paid = await sender.Send(new MarkOrderPaidCommand(prepared.Value.OrderNumber, reference), ct);

                return paid.IsFailure ? paid.ToHttp() : prepared.ToHttp();
            })
            .RequireAuthorization()
            .WithTags("Orders")
            .WithName("PayOrderWithTestPayment")
            .WithSummary("TEST ONLY: pay my order without paying (no money moves); the seller then sees it")
            .WithDescription("Temporary, until Paystack / Stripe are connected. Exists only while " +
                             "Payments:TestPaymentsEnabled is true. Returns what a real payment would have charged.")
            .Produces<OrderPaymentDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ═════════════════════════════ C. My shop ═════════════════════════════

    private static void MapMyShop(IEndpointRouteBuilder api)
    {
        var shopOrders = api.MapGroup("/shops/mine/orders")
                            .WithTags("My shop orders")
                            .RequireAuthorization();

        // ───────────────────────────── 1. Lists ─────────────────────────────

        shopOrders.MapGet("/", async ([AsParameters] GetShopOrdersQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetMyShopOrders")
            .WithSummary("My shop's orders, newest first (status=Paid → \"New orders\"); unpaid orders never appear")
            .Produces<PagedResult<ShopOrderSummaryDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        shopOrders.MapGet("/stats", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetShopOrderStatsQuery(), ct)).ToHttp())
            .WithName("GetMyShopOrderStats")
            .WithSummary("Seller dashboard: orders per status and this week's daily sales")
            .Produces<ShopOrderStatsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ───────────────────────────── 2. Moving an order along ─────────────────────────────

        shopOrders.MapPost("/{orderNumber}/accept", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new AcceptOrderCommand(orderNumber), ct)).ToHttp())
            .WithName("AcceptShopOrder")
            .WithSummary("Accept a paid order and start preparing it (Paid → Preparing)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        shopOrders.MapPost("/{orderNumber}/decline",
                async (string orderNumber, DeclineRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new DeclineOrderCommand(orderNumber, body.Reason), ct)).ToHttp())
            .WithName("DeclineShopOrder")
            .WithSummary("Can't make a new order: it's cancelled and refunded, and the buyer is emailed the reason")
            .WithDescription("Only a Paid order (not yet accepted). The team is told if a shop declines more than " +
                             "3 orders in 7 days.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        shopOrders.MapPost("/{orderNumber}/send-out", async (string orderNumber, ISender sender,
                                                             CancellationToken ct) =>
                (await sender.Send(new SendOutOrderCommand(orderNumber), ct)).ToHttp())
            .WithName("SendOutShopOrder")
            .WithSummary("The order has left the kitchen (Preparing → OutForDelivery)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        shopOrders.MapPost("/{orderNumber}/delivered", async (string orderNumber, ISender sender,
                                                              CancellationToken ct) =>
                (await sender.Send(new MarkOrderDeliveredCommand(orderNumber), ct)).ToHttp())
            .WithName("MarkShopOrderDelivered")
            .WithSummary("Delivered (OutForDelivery → Delivered): commission taken, earnings added to my balance")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "decline" (the order comes from the URL): why, shown to the buyer.</summary>
    public sealed record DeclineRequest(string Reason);
}
