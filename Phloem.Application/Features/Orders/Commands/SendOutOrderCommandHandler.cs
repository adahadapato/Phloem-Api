using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="SendOutOrderCommand"/>.</summary>
public sealed class SendOutOrderCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    CourierDirectory couriers,
    DeliveryEmails emails,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<SendOutOrderCommand, Result>
{
    /// <summary>Only the order's own shop can send it out; the courier must deliver in the order's country.</summary>
    /// <returns>Success; or <see cref="VendorErrors.NotFound"/>, <see cref="OrderErrors.NotFound"/>,
    /// <see cref="DeliveryErrors.CourierNotFound"/>, <see cref="DeliveryErrors.CourierInAnotherCountry"/>,
    /// <see cref="OrderErrors.InvalidStatusChange"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(SendOutOrderCommand command, CancellationToken ct)
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

        var moved = order.SendOut(courier.Value.Id, courier.Value.Name, time.GetUtcNow().UtcDateTime);
        if (moved.IsFailure) return moved;

        await unitOfWork.SaveChangesAsync(ct);

        if (courier.Value.Id != userId)
            await emails.NewDeliveryAsync(courier.Value.Id, order.OrderNumber, $"A food order from {order.VendorName}",
                                          order.DeliveryAddressLine, ct);
        return Result.Success();
    }
}

/// <summary>"Who delivers it" for a seller: themselves (no courier, or their own Id), or a courier in the country.</summary>
internal static class ShopCourier
{
    /// <summary>Works out the courier's Id and name.</summary>
    /// <param name="courierId">The chosen courier; null = the seller.</param>
    /// <param name="sellerId">The signed-in seller.</param>
    /// <param name="country">The order's delivery country.</param>
    /// <param name="couriers">The courier directory.</param>
    /// <param name="profiles">Profiles (for the seller's own name).</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Id and name; or a <see cref="DeliveryErrors"/> error.</returns>
    public static async Task<Result<(Guid Id, string Name)>> ResolveAsync(
        Guid? courierId, Guid sellerId, Country country, CourierDirectory couriers, IUserProfileRepository profiles,
        CancellationToken ct)
    {
        if (courierId is null || courierId == sellerId)
        {
            var names = await profiles.GetFullNamesAsync([sellerId], ct);
            return Result<(Guid Id, string Name)>.Success((sellerId, names.GetValueOrDefault(sellerId, "The shop")));
        }

        var found = await couriers.FindAsync(courierId.Value, country, ct);
        if (found.IsFailure) return found.Error;
        return Result<(Guid Id, string Name)>.Success((found.Value.Id, found.Value.GetFullName()));
    }
}
