using Phloem.Domain.Enums;

namespace Phloem.Domain.Models;

/// <summary>Admin → Orders: what to show (every part optional).</summary>
/// <param name="Country">Only orders in this country.</param>
/// <param name="ShopId">Only this shop's orders.</param>
/// <param name="Status">Only orders in this status.</param>
/// <param name="FromUtc">Only orders placed from this moment.</param>
/// <param name="ToUtc">Only orders placed before this moment.</param>
/// <param name="Search">An order number, or part of the customer's or shop's name.</param>
public sealed record AdminOrderFilter(
    Country? Country = null,
    Guid? ShopId = null,
    OrderStatus? Status = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string? Search = null);
