namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>One dish on the Checkout page, priced by the server right now.</summary>
/// <param name="DishId">The dish.</param>
/// <param name="DishName">Its name, e.g. "Jollof Rice &amp; Chicken".</param>
/// <param name="ImageUrl">Thumbnail URL, or null if the dish has no photo.</param>
/// <param name="Quantity">How many.</param>
/// <param name="UnitPrice">Today's price per item.</param>
/// <param name="LineTotal">UnitPrice × Quantity; 0 if not available.</param>
/// <param name="IsAvailable">False = can't be ordered right now (shown as "Out of stock").</param>
public sealed record BasketLineDto(
    Guid DishId,
    string DishName,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    bool IsAvailable);