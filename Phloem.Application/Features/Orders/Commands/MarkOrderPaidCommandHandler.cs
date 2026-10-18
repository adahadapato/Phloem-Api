using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="MarkOrderPaidCommand"/>.</summary>
public sealed class MarkOrderPaidCommandHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<MarkOrderPaidCommandHandler> logger)
    : IRequestHandler<MarkOrderPaidCommand, Result>
{
    /// <summary>Marks the order paid and saves.</summary>
    /// <returns>Success; or <see cref="OrderErrors.NotFound"/>, <see cref="OrderErrors.NotAwaitingPayment"/>.</returns>
    public async Task<Result> Handle(MarkOrderPaidCommand command, CancellationToken ct)
    {
        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null) return OrderErrors.NotFound;

        var paid = order.MarkPaid(command.PaymentReference, time.GetUtcNow().UtcDateTime);
        if (paid.IsFailure) return paid;

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} paid ({PaymentReference}).",
                              order.OrderNumber, command.PaymentReference.Trim());
        return Result.Success();
    }
}
