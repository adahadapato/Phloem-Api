using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Application.Features.Orders.Common;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="GetOrderQuery"/>.</summary>
public sealed class GetOrderQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IVendorRepository vendors,
    IDishRepository dishes,
    IDeliveryPricingRepository pricings,
    IUserProfileRepository profiles,
    IFileStorage storage,
    IAccountService accounts,
    DeliveryPhotos photos,
    TimeProvider time)
    : IRequestHandler<GetOrderQuery, Result<OrderDetailsDto>>
{
    private const string UnknownUser = "Unknown user";

    /// <summary>
    /// Loads the order, checks the viewer may see it, and builds the details
    /// (live prices for an unpaid order).
    /// </summary>
    /// <returns>The details; or <see cref="OrderErrors.NotFound"/> (also for orders the viewer may not see).</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<OrderDetailsDto>> Handle(GetOrderQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var order = await orders.GetByOrderNumberAsync(query.OrderNumber.Trim(), ct);
        if (order is null) return OrderErrors.NotFound;

        // Who's looking: the customer, or the shop (paid orders only)
        var isCustomer = order.CustomerId == userId;
        var myShop = isCustomer ? null : await vendors.GetByOwnerIdAsync(userId, ct);
        var isSeller = myShop is not null
                       && myShop.Id == order.VendorId
                       && order.Status != OrderStatus.AwaitingPayment;

        if (!isCustomer && !isSeller) return OrderErrors.NotFound;   // never reveal other people's orders

        // Live data for an unpaid order
        var current = new Dictionary<Guid, Dish>();
        Vendor? vendor = null;
        DeliveryPricing? pricing = null;

        if (LiveOrderPricing.IsLive(order))
        {
            current = (await dishes.GetByIdsAsync(order.Lines.Select(l => l.DishId).ToList(), ct))
                      .ToDictionary(d => d.Id);
            vendor = await vendors.GetByIdAsync(order.VendorId, ct);
            if (vendor is not null) pricing = await pricings.GetByCountryAsync(vendor.Country, ct);
        }

        var lines = LiveOrderPricing.Lines(order, current, storage);
        var subtotal = lines.Sum(l => l.LineTotal);
        var fee = LiveOrderPricing.DeliveryFee(order, vendor, pricing);

        var names = await profiles.GetFullNamesAsync([order.CustomerId], ct);
        var customerName = names.GetValueOrDefault(order.CustomerId, UnknownUser);
        // Address saved without a phone (or before phones existed): the customer's own
        var phone = order.DeliveryPhone;
        if (phone is null)
        {
            var contact = await accounts.GetContactAsync(order.CustomerId, ct);
            phone = contact.IsSuccess ? contact.Value.PhoneNumber : null;
        }
        var now = time.GetUtcNow().UtcDateTime;
        var secondsLeft = order.TimeLeftToCancel(now);
        var proof = await photos.ToDtoAsync(order.DeliveryProof, order.CourierName, ct);

        return new OrderDetailsDto(
            order.OrderNumber,
            order.VendorName,
            customerName,
            order.Status,
            lines,
            subtotal,
            fee,
            subtotal + fee,
            order.Currency,
            order.DeliveryWindow,
            order.DeliveryLabel,
            order.DeliveryAddressLine,
            order.PlacedAtUtc,
            order.PaidAtUtc,
            order.DeliveredAtUtc,
            order.CancelledAtUtc,
            CanCancel: isCustomer && order.CanCancel(now),
            CancelSecondsLeft: secondsLeft is null ? null : (int)secondsLeft.Value.TotalSeconds,
            order.NeedsRefund,
            ViewedBySeller: isSeller,
            order.DeclinedByShop,
            order.DeclineReason,
            order.DeliveryRecipientName ?? customerName,
            phone,
            order.CourierName,
            proof,
            CanMarkDelivered: order.Status == OrderStatus.OutForDelivery && order.CourierId == userId);
    }
}