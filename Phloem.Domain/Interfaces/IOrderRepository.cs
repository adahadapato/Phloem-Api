using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Models;
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

    /// <summary>
    /// Seller dashboard → weekly sales chart: how many orders the shop was PAID for on each day
    /// (by <see cref="Order.PaidAtUtc"/>, UTC date), cancelled orders excluded.
    /// Days with no orders are simply missing from the result.
    /// </summary>
    /// <param name="vendorId">The seller's shop.</param>
    /// <param name="fromDate">First day (inclusive).</param>
    /// <param name="toDate">Last day (inclusive).</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyDictionary<DateOnly, int>> GetDailyPaidCountsForVendorAsync(
        Guid vendorId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

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
    /// Orders tab → "My orders": the customer's orders (unpaid ones included), newest first, with lines,
    /// narrowed down by the tabs, the time choice and the search box.
    /// </summary>
    /// <param name="customerId">The signed-in customer.</param>
    /// <param name="filter">Statuses, placed-since and search text (<see cref="CustomerOrderFilter.None"/> = all).</param>
    /// <param name="page">Which page and how many per page.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<PagedResult<Order>> GetForCustomerAsync(
        Guid customerId, CustomerOrderFilter filter, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// "Buy it again": the dishes from the customer's PAID orders (cancelled ones included: they may still want
    /// them), most recently ordered first, each dish once.
    /// </summary>
    /// <param name="customerId">The signed-in customer.</param>
    /// <param name="take">How many dish Ids at most.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>Dish Ids, newest first. Dishes may since have been deleted: the caller checks.</returns>
    Task<IReadOnlyList<Guid>> GetRecentDishIdsForCustomerAsync(
        Guid customerId, int take, CancellationToken cancellationToken = default);

    // ───────────────────────────── Suggestions ("For you") ─────────────────────────────

    /// <summary>
    /// Suggestions: every dish in the customer's most recent paid orders (cancelled ones left out), once each, with
    /// its kind of food, shop, how many orders had it and when it was last bought. Deleted dishes are included
    /// (they still say what the customer likes).
    /// </summary>
    /// <param name="customerId">The signed-in customer.</param>
    /// <param name="maxOrders">How many of their newest orders to look at.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>Most recently ordered first.</returns>
    Task<IReadOnlyList<PurchasedDish>> GetPurchasedDishesAsync(
        Guid customerId, int maxOrders, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggestions: "people who ordered X also ordered Y". Looks at every customer's paid orders (not cancelled)
    /// delivered to the country since a date that contain any of the given dishes, and counts the OTHER dishes in them.
    /// </summary>
    /// <param name="dishIds">Dishes the customer has bought (X).</param>
    /// <param name="country">Only orders delivered in this country.</param>
    /// <param name="sinceUtc">Only orders placed since this moment.</param>
    /// <param name="take">How many pairs at most (the most frequent).</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>Pairs, most frequent first. Y is never one of <paramref name="dishIds"/>.</returns>
    Task<IReadOnlyList<DishPairCount>> GetAlsoOrderedAsync(
        IReadOnlyCollection<Guid> dishIds, Country country, DateTime sinceUtc, int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggestions: the dishes in the most paid orders (not cancelled) delivered to a country since a date
    /// ("Popular in Ghana").
    /// </summary>
    /// <param name="country">Only orders delivered in this country.</param>
    /// <param name="sinceUtc">Only orders placed since this moment.</param>
    /// <param name="take">How many dish Ids at most.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>Dish Ids, most ordered first. Some may no longer be on sale: the caller checks.</returns>
    Task<IReadOnlyList<Guid>> GetMostOrderedDishIdsAsync(
        Country country, DateTime sinceUtc, int take, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// The shop's orders that haven't left it yet (awaiting payment, paid, being prepared), tracked, so they can be
    /// cancelled when the owner is suspended.
    /// </summary>
    /// <param name="vendorId">The shop.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<Order>> GetUnfinishedForVendorAsync(
        Guid vendorId, CancellationToken cancellationToken = default);

    /// <summary>How many orders the shop has declined since a time (team alert when a shop declines too often).</summary>
    /// <param name="vendorId">The shop.</param>
    /// <param name="sinceUtc">Count declines from this time.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<int> CountDeclinedForVendorSinceAsync(
        Guid vendorId, DateTime sinceUtc, CancellationToken cancellationToken = default);
}