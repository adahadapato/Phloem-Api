namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// One dish in an order. For an UNPAID order the price is today's price, with notes
/// ("price changed", "out of stock"); for a paid order it is the price that was paid.
/// </summary>
/// <param name="DishId">The dish (for "Buy again"; never shown).</param>
/// <param name="DishName">Its name.</param>
/// <param name="ImageUrl">Thumbnail URL, or null.</param>
/// <param name="Quantity">How many.</param>
/// <param name="UnitPrice">Unpaid: today's price. Paid: the price paid.</param>
/// <param name="PriceWhenAdded">Unpaid only: the price when it was added; null for paid orders.</param>
/// <param name="PriceChanged">Unpaid only: true if today's price differs from <paramref name="PriceWhenAdded"/>.</param>
/// <param name="IsOutOfStock">Unpaid only: true if the dish can't be ordered right now.</param>
/// <param name="LineTotal">UnitPrice × Quantity; 0 if out of stock.</param>
public sealed record OrderLineDto(
    Guid DishId,
    string DishName,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal? PriceWhenAdded,
    bool PriceChanged,
    bool IsOutOfStock,
    decimal LineTotal);