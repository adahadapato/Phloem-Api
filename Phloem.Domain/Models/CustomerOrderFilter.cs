using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Domain.Models;

/// <summary>
/// What "My orders" is narrowed down by (the tabs, the time choice and the search box).
/// Passed to <see cref="IOrderRepository.GetForCustomerAsync"/>. Every part is optional.
/// </summary>
/// <param name="Statuses">Only these statuses; null = every status.</param>
/// <param name="PlacedFromUtc">Only orders placed at or after this moment (UTC); null = any time.</param>
/// <param name="Search">Text found in the order number, the shop's name or a dish's name; null = no search.</param>
public sealed record CustomerOrderFilter(
    IReadOnlyCollection<OrderStatus>? Statuses = null,
    DateTime? PlacedFromUtc = null,
    string? Search = null)
{
    /// <summary>No filter: every order.</summary>
    public static readonly CustomerOrderFilter None = new();

    /// <summary>Longest search text accepted (matches the API's validation).</summary>
    public const int SearchMaxLength = 100;
}
