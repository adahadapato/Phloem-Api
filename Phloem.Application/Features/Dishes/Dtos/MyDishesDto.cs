namespace Phloem.Application.Features.Dishes.Dtos;

/// <summary>
/// Seller dashboard → "My dishes", plus the banner count:
/// "2 dishes need a price update" (0 = no banner).
/// </summary>
public sealed record MyDishesDto(IReadOnlyList<MyDishDto> Dishes, int NeedsPriceUpdateCount);