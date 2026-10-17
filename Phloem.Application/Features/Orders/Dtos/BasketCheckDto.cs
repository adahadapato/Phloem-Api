namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// Checkout page: the app's basket checked and priced by the server, split by shop.
/// </summary>
/// <param name="Shops">One entry per shop (each becomes its own order).</param>
/// <param name="MissingDishIds">
/// Dishes in the app's basket that no longer exist at all; the app should remove them.
/// </param>
/// <param name="CanPlaceOrder">
/// True only if every dish is available and every shop's delivery could be priced.
/// </param>
public sealed record BasketCheckDto(
    IReadOnlyList<BasketShopDto> Shops,
    IReadOnlyList<Guid> MissingDishIds,
    bool CanPlaceOrder);