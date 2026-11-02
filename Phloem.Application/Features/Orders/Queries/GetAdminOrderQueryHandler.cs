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

/// <summary>Handles <see cref="GetAdminOrderQuery"/>.</summary>
public sealed class GetAdminOrderQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IUserProfileRepository profiles,
    IAccountService accounts,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    IFileStorage storage,
    DeliveryPhotos photos,
    OrderTimeline timeline)
    : IRequestHandler<GetAdminOrderQuery, Result<AdminOrderDetailsDto>>
{
    private const string UnknownUser = "Unknown user";

    /// <summary>Loads the order, checks the person covers its country, records the view, builds the details.</summary>
    /// <returns>The details; or <see cref="OrderErrors.NotFound"/> (also for orders outside the person's countries).</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<AdminOrderDetailsDto>> Handle(GetAdminOrderQuery query, CancellationToken ct)
    {
        var staffId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var order = await orders.GetWithTimelineAsync(query.OrderNumber.Trim(), ct);
        if (order is null || !currentUser.CoversCountry(order.DeliveryCountry)) return OrderErrors.NotFound;

        // Who opened what: staff can see a customer's address and phone, so every open is recorded
        audit.Add(StaffAuditEntry.Create(staffId, StaffAction.OrderViewed, $"Order {order.OrderNumber}",
                                         order.DeliveryCountry));
        await unitOfWork.SaveChangesAsync(ct);

        var names = await profiles.GetFullNamesAsync([order.CustomerId], ct);
        var customerName = names.GetValueOrDefault(order.CustomerId, UnknownUser);
        var contact = await accounts.GetContactAsync(order.CustomerId, ct);

        // The prices frozen on the order (staff never see "live" prices)
        var lines = order.Lines.Select(l => new OrderLineDto(
            l.DishId, l.DishName, l.DishImagePath is null ? null : storage.GetPublicUrl(l.DishImagePath),
            l.Quantity, l.UnitPrice, PriceWhenAdded: null, PriceChanged: false, IsOutOfStock: false,
            l.LineTotal)).ToList();

        var proof = await photos.ToDtoAsync(order.DeliveryProof, order.CourierName, ct);
        var steps = await timeline.BuildAsync(order, forStaff: true, ct);

        return new AdminOrderDetailsDto(
            order.OrderNumber,
            order.Status,
            order.DeliveryCountry,
            order.VendorId,
            order.VendorName,
            order.CustomerId,
            customerName,
            contact.IsSuccess ? contact.Value.Email : null,
            contact.IsSuccess ? contact.Value.PhoneNumber : null,
            lines,
            order.Subtotal,
            order.DeliveryFee,
            order.Total,
            order.Currency,
            order.CommissionAmount,
            order.SellerEarnings,
            order.DeliveryWindow,
            order.DeliveryLabel,
            order.DeliveryAddressLine,
            order.DeliveryRecipientName ?? customerName,
            order.DeliveryPhone,
            order.PlacedAtUtc,
            order.PaidAtUtc,
            order.PaymentReference,
            order.DeliveredAtUtc,
            order.CancelledAtUtc,
            order.NeedsRefund,
            order.DeclinedByShop,
            order.DeclineReason,
            order.CourierId,
            order.CourierName,
            proof,
            steps);
    }
}
