using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.DishCatalogs.Dtos;

/// <summary>
/// Admin → "Dish name suggestions": "Kelewele · Snacks · suggested by Ama Mensah (Mama Ama's Kitchen)".
/// Id is only for the Approve / Reject buttons; nothing here is shown as an Id.
/// </summary>
public sealed record AdminDishSuggestionDto(
    Guid Id,
    string Name,
    DishCategory Category,
    CatalogGroup Group,
    string SuggestedBy,
    string? ShopName,
    DateTime SuggestedAt)
{
    public static AdminDishSuggestionDto From(DishCatalog item, string suggestedBy, string? shopName) => new(
        item.Id,
        item.Name,
        item.Category,
        item.Group,
        suggestedBy,
        shopName,
        item.CreatedAt);
}