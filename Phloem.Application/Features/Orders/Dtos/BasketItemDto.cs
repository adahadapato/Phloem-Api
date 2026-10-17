namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// One line of the app's basket, as SENT by the app. Only the dish and quantity: prices,
/// names and availability are always worked out by the server, never trusted from the app.
/// </summary>
/// <param name="DishId">The dish.</param>
/// <param name="Quantity">How many; 1 to Order.MaxQuantityPerDish.</param>
public sealed record BasketItemDto(Guid DishId, int Quantity);