using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Loads and saves <see cref="Order"/> aggregates (an order with its lines).
/// Implemented in Persistence with EF Core.
/// </summary>
/// <remarks>
/// <see cref="IRepository{TEntity}.GetByIdAsync"/> includes the order lines (Order details page).
/// Unpaid orders (<see cref="OrderStatus.AwaitingPayment"/>) are NEVER returned to sellers.
/// </remarks>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Order details when someone taps an order number (e.g. "PH-240518-4821").</summary>
    /// <param name="orderNumber">The order number, as shown in the app.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>The order with its lines, or null if there is none.</returns>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>Checks a newly generated order number isn't already used.</summary>
    /// <param name="orderNumber">The candidate number.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Orders tab → "My orders": ALL the customer's orders (unpaid ones included), newest first, with lines.
    /// </summary>
    /// <param name="customerId">The signed-in customer.</param>
    /// <param name="page">Which page and how many per page.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<PagedResult<Order>> GetForCustomerAsync(
        Guid customerId, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seller → "Shop orders": this shop's orders, newest first, with lines.
    /// Never includes unpaid orders. Optionally only one status (e.g. Paid = waiting to be accepted).
    /// </summary>
    /// <param name="vendorId">The seller's shop (<see cref="Order.VendorId"/>).</param>
    /// <param name="status">Only this status; null = every status except AwaitingPayment.</param>
    /// <param name="page">Which page and how many per page.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<PagedResult<Order>> GetForVendorAsync(
        Guid vendorId, OrderStatus? status, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seller dashboard tiles ("New orders", "Total orders"), counted live so they can never
    /// drift out of date. Never counts unpaid orders.
    /// </summary>
    /// <param name="vendorId">The seller's shop.</param>
    /// <param name="status">Only this status; null = every status except AwaitingPayment.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<int> CountForVendorAsync(
        Guid vendorId, OrderStatus? status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Weekly price-change email job: unpaid orders, with lines, in batches (oldest first),
    /// so the job never loads every order at once.
    /// </summary>
    /// <param name="skip">How many to skip (batch number × batch size).</param>
    /// <param name="take">Batch size.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<Order>> GetAwaitingPaymentAsync(
        int skip, int take, CancellationToken cancellationToken = default);
}