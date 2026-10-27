using Phloem.Application.Features.Dishes.Dtos;

namespace Phloem.Application.Features.Suggestions.Dtos;

/// <summary>
/// <c>GET /api/me/suggestions</c>: the two rows on Home for a signed-in customer.
/// </summary>
/// <param name="OrderAgain">Dishes they've bought that can be ordered now in this country, most recent first.</param>
/// <param name="ForYou">Dishes they haven't tried, each with the reason it was picked.</param>
/// <param name="IsPersonal">
/// False when there's no order history yet: <paramref name="ForYou"/> is then just "Popular in Ghana", so the
/// screen can title the row "Popular in Ghana" instead of "For you".
/// </param>
public sealed record SuggestionsDto(
    IReadOnlyList<DishCardDto> OrderAgain,
    IReadOnlyList<SuggestedDishDto> ForYou,
    bool IsPersonal);

/// <summary>A suggested dish: the usual dish card, plus a short reason shown on it.</summary>
/// <param name="Dish">The same card as Explore (so "+" puts it straight in the basket).</param>
/// <param name="Reason">e.g. "Often ordered with Jollof Rice", "Popular in Ghana".</param>
public sealed record SuggestedDishDto(DishCardDto Dish, string Reason);
