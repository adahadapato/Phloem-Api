using Phloem.Domain.Enums;

namespace Phloem.Domain.Models;

/// <summary>
/// One dish from a customer's paid orders, for "For you" suggestions: what kind of food it is, which shop sold it,
/// how many of their orders had it and when it was last bought. Read by
/// <c>IOrderRepository.GetPurchasedDishesAsync</c>.
/// </summary>
/// <param name="DishId">The dish (it may since have been deleted or switched off).</param>
/// <param name="DishName">Its name as on the order.</param>
/// <param name="DishCatalogId">Its name in the dish list (the same name from another shop shares this).</param>
/// <param name="Category">Local meal, snack or drink.</param>
/// <param name="Group">Ghanaian, Nigerian, West African, snacks or drinks.</param>
/// <param name="VendorId">The shop it was bought from.</param>
/// <param name="VendorName">The shop's name as on the order.</param>
/// <param name="Orders">How many of the customer's orders had it.</param>
/// <param name="LastOrderedUtc">When it was last ordered.</param>
public sealed record PurchasedDish(
    Guid DishId,
    string DishName,
    Guid DishCatalogId,
    DishCategory Category,
    CatalogGroup Group,
    Guid VendorId,
    string VendorName,
    int Orders,
    DateTime LastOrderedUtc);
