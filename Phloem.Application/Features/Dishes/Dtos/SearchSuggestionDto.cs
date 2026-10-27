namespace Phloem.Application.Features.Dishes.Dtos;

/// <summary>What a search prediction points at.</summary>
public enum SearchSuggestionKind
{
    /// <summary>A dish name: picking it searches for it.</summary>
    Dish = 1,

    /// <summary>A shop: picking it opens the shop.</summary>
    Shop = 2
}

/// <summary>
/// One line in the search box's dropdown (<c>GET /api/dishes/suggest</c>): "Jollof Rice · 7 dishes" or a shop.
/// </summary>
/// <param name="Text">The full name to show (and to search for, for a dish).</param>
/// <param name="Kind">Dish or Shop.</param>
/// <param name="DishCount">Dish: how many dishes have this name. Shop: how many it sells (not counted: 0).</param>
/// <param name="ShopId">Shop only: where to go.</param>
public sealed record SearchSuggestionDto(string Text, SearchSuggestionKind Kind, int DishCount, Guid? ShopId);
