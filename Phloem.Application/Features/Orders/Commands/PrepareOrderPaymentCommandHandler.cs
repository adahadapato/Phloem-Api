using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="PrepareOrderPaymentCommand"/>.</summary>
public sealed class PrepareOrderPaymentCommandHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IVendorRepository vendors,
    IDishRepository dishes,
    IDeliveryPricingRepository pricings,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PrepareOrderPaymentCommand, Result<OrderPaymentDto>>
{
    /// <summary>Loads today's dishes and fee, lets the Domain refresh / drop lines, and saves.</summary>
    /// <returns>
    /// The amount and removed dishes; or <see cref="OrderErrors.NotFound"/>,
    /// <see cref="OrderErrors.NotAwaitingPayment"/>, <see cref="OrderErrors.NothingToPay"/>,
    /// <see cref="DeliveryPricingErrors.NotSet"/>, <see cref="DeliveryPricingErrors.WindowNotOffered"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<OrderPaymentDto>> Handle(PrepareOrderPaymentCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null || order.CustomerId != userId) return OrderErrors.NotFound;

        // The shop is gone (closed) → every dish is out of stock
        var vendor = await vendors.GetByIdAsync(order.VendorId, ct);
        if (vendor is null) return OrderErrors.NothingToPay;

        var pricing = await pricings.GetByCountryAsync(vendor.Country, ct);
        if (pricing is null) return DeliveryPricingErrors.NotSet;

        var fee = pricing.FeeFor(vendor, order.DeliveryWindow);
        if (fee.IsFailure) return fee.Error;

        var current = await dishes.GetByIdsAsync(order.Lines.Select(l => l.DishId).ToList(), ct);

        var prepared = order.PrepareForPayment(current, fee.Value);
        if (prepared.IsFailure) return prepared.Error;

        await unitOfWork.SaveChangesAsync(ct);

        return new OrderPaymentDto(order.OrderNumber, order.Total, order.Currency, prepared.Value);
    }
}