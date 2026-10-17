using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="MarkOrderDeliveredCommand"/>.</summary>
public sealed class MarkOrderDeliveredCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    ICommissionRepository commissions,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<MarkOrderDeliveredCommand, Result>
{
    /// <summary>
    /// Marks delivered (Domain stores commission + earnings), credits the shop's balance, and saves
    /// both in ONE save, so the order and the balance can never disagree.
    /// </summary>
    /// <returns>Success; or <see cref="VendorErrors.NotFound"/>, <see cref="OrderErrors.NotFound"/>,
    /// <see cref="OrderErrors.InvalidStatusChange"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(MarkOrderDeliveredCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null || order.VendorId != vendor.Id || order.Status == OrderStatus.AwaitingPayment)
            return OrderErrors.NotFound;

        // The rate in force NOW (null = none set = 0%)
        var commission = await commissions.GetByCountryAsync(order.DeliveryCountry, ct);

        var delivered = order.MarkDelivered(time.GetUtcNow().UtcDateTime, commission);
        if (delivered.IsFailure) return delivered;

        if (order.SellerEarnings is > 0)
            vendor.Credit(order.SellerEarnings.Value, order.Currency);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}