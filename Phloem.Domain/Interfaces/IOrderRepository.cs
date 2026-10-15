using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared;

namespace Phloem.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    // GetByIdAsync (inherited) includes the order lines → Order details page.

    /// <summary>Order details when the customer taps an order number (e.g. "PH-240518-4821").</summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>Order numbers must be unique: checked when generating a new one.</summary>
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>Orders tab → "My orders": the customer's orders, newest first, with lines.</summary>
    Task<PagedResult<Order>> GetForCustomerAsync(
        Guid customerId, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seller dashboard → "Shop orders": orders containing at least one of this shop's dishes,
    /// newest first, optionally only one status (e.g. Placed = waiting to be accepted).
    /// </summary>
    Task<PagedResult<Order>> GetForVendorAsync(
        Guid vendorId, OrderStatus? status, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seller dashboard stat tiles ("Pending orders", "Total orders"):
    /// counted live instead of stored on Vendor, so they can never drift out of date.
    /// </summary>
    Task<int> CountForVendorAsync(
        Guid vendorId, OrderStatus? status, CancellationToken cancellationToken = default);
}