namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// "Buy again": what to put back in the app's basket. The app then opens Checkout,
/// which prices everything at today's prices (CheckBasketQuery).
/// </summary>
/// <param name="Items">Dishes that can still be ordered, with the old quantities.</param>
/// <param name="UnavailableDishes">Names of dishes that can't be ordered any more.</param>
public sealed record BuyAgainDto(IReadOnlyList<BasketItemDto> Items, IReadOnlyList<string> UnavailableDishes);