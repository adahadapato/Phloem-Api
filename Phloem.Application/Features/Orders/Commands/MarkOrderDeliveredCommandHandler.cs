using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="MarkOrderDeliveredCommand"/>.</summary>
public sealed class MarkOrderDeliveredCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    ICommissionRepository commissions,
    DeliveryPhotos photos,
    DeliveryEmails emails,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<MarkOrderDeliveredCommandHandler> logger)
    : IRequestHandler<MarkOrderDeliveredCommand, Result>
{
    /// <summary>
    /// Only the order's courier: saves the photo (private), marks delivered (Domain stores the proof, commission and
    /// earnings), credits the shop's balance and saves both in ONE save, so the order and the balance can never
    /// disagree. If anything is refused, the photo is removed again.
    /// </summary>
    /// <returns>Success; or <see cref="DeliveryErrors.NotYourDelivery"/> (also for orders that don't exist),
    /// <see cref="OrderErrors.InvalidStatusChange"/>, <see cref="DeliveryErrors.SayWhichNeighbour"/>, an
    /// <c>ImageErrors</c> error.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(MarkOrderDeliveredCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        // 1. Only the courier it was given to (anyone else: "not found", so nobody learns about other orders)
        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null || order.CourierId != userId) return DeliveryErrors.NotYourDelivery;
        if (order.Status != OrderStatus.OutForDelivery)
            return OrderErrors.InvalidStatusChange(order.Status, OrderStatus.Delivered);

        // 2. The photo (checked before anything changes)
        var saved = await photos.SaveAsync(command.Photo, "orders", order.OrderNumber, ct);
        if (saved.IsFailure) return saved.Error;

        var proof = ProofOfDelivery.Create(saved.Value, command.Handover, command.ReceivedBy, command.Note, userId,
                                           time.GetUtcNow().UtcDateTime);
        if (proof.IsFailure)
        {
            await photos.DeleteAsync(saved.Value, ct);
            return proof.Error;
        }

        // 3. Delivered: the rate in force NOW (null = none set = 0%)
        var commission = await commissions.GetByCountryAsync(order.DeliveryCountry, ct);
        var delivered = order.MarkDelivered(proof.Value, commission);
        if (delivered.IsFailure)
        {
            await photos.DeleteAsync(saved.Value, ct);
            return delivered;
        }

        // 4. The shop's earnings
        var vendor = await vendors.GetByIdAsync(order.VendorId, ct);
        if (vendor is null)
            logger.LogError("Order {OrderNumber} delivered but its shop {VendorId} wasn't found: earnings of {Amount} " +
                            "NOT credited.", order.OrderNumber, order.VendorId, order.SellerEarnings);
        else if (order.SellerEarnings is > 0)
            vendor.Credit(order.SellerEarnings.Value, order.Currency);

        await unitOfWork.SaveChangesAsync(ct);

        await emails.OrderDeliveredAsync(order, ct);
        return Result.Success();
    }
}
