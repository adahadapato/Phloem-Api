using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="ChangeOrderCourierCommand"/>.</summary>
public sealed class ChangeOrderCourierCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    CourierDirectory couriers,
    DeliveryEmails emails,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<ChangeOrderCourierCommand, Result>
{
    /// <returns>Success; or <see cref="VendorErrors.NotFound"/>, <see cref="OrderErrors.NotFound"/>,
    /// <see cref="DeliveryErrors.CourierNotFound"/>, <see cref="DeliveryErrors.CourierInAnotherCountry"/>,
    /// <see cref="DeliveryErrors.NotOutForDelivery"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(ChangeOrderCourierCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null || order.VendorId != vendor.Id || order.Status == OrderStatus.AwaitingPayment)
            return OrderErrors.NotFound;

        var courier = await ShopCourier.ResolveAsync(command.CourierId, userId, order.DeliveryCountry,
                                                     couriers, profiles, ct);
        if (courier.IsFailure) return courier.Error;

        var previous = order.CourierId;
        var changed = order.ChangeCourier(courier.Value.Id, courier.Value.Name, time.GetUtcNow().UtcDateTime);
        if (changed.IsFailure) return changed;

        await unitOfWork.SaveChangesAsync(ct);

        if (courier.Value.Id != userId && courier.Value.Id != previous)
            await emails.NewDeliveryAsync(courier.Value.Id, order.OrderNumber, $"A food order from {order.VendorName}",
                                          order.DeliveryAddressLine, ct);
        return Result.Success();
    }
}
