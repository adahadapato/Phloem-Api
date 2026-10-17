using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="AcceptOrderCommand"/>.</summary>
public sealed class AcceptOrderCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AcceptOrderCommand, Result>
{
    /// <summary>Only the order's own shop can accept it, and only once it's paid.</summary>
    /// <returns>Success; or <see cref="VendorErrors.NotFound"/>, <see cref="OrderErrors.NotFound"/>,
    /// <see cref="OrderErrors.InvalidStatusChange"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(AcceptOrderCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null || order.VendorId != vendor.Id || order.Status == OrderStatus.AwaitingPayment)
            return OrderErrors.NotFound;

        var moved = order.StartPreparing();
        if (moved.IsFailure) return moved;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}