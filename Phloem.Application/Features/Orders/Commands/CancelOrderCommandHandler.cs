using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="CancelOrderCommand"/>.</summary>
public sealed class CancelOrderCommandHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<CancelOrderCommand, Result>
{
    /// <summary>Cancels the customer's own order.</summary>
    /// <returns>Success; or <see cref="OrderErrors.NotFound"/>, <see cref="OrderErrors.CancelWindowPassed"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(CancelOrderCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null || order.CustomerId != userId) return OrderErrors.NotFound;

        var cancelled = order.Cancel(time.GetUtcNow().UtcDateTime);
        if (cancelled.IsFailure) return cancelled;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}