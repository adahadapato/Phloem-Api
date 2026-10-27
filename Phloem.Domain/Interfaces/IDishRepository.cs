using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

public interface IDishRepository : IRepository<Dish>
{
    // GetByIdAsync (inherited) includes the photos and the vendor (needed for CanBeOrdered).

    /// <summary>
    /// Food listing (Explore tab): approved shops' available dishes only.
    /// Filters match the app: country chip, category chip, search box, and "View all" for one shop.
    /// Search matches the dish name or the shop name.
    /// </summary>
    Task<PagedResult<Dish>> SearchAsync(
        Country? country, DishCategory? category, string? search, Guid? vendorId, PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Seller dashboard → "My dishes": ALL of the shop's dishes (not deleted), including ones
    /// switched off or needing a price update. Includes photos and DishCatalog with PriceRanges.
    /// </summary>
    Task<IReadOnlyList<Dish>> GetByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Place order / Buy again: the basket's dishes in one query (with vendor, for the snapshot).
    /// Missing or deleted Ids are simply not returned, so the caller can say "no longer available".
    /// </summary>
    Task<IReadOnlyList<Dish>> GetByIdsAsync(
        IReadOnlyCollection<Guid> dishIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vendors list cards: "12 dishes". One query for a whole page of shops
    /// (shops with no dishes are simply missing from the result).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> CountByVendorsAsync(
        IReadOnlyCollection<Guid> vendorIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin changed a price range: every dish with that name in that country
    /// (with its Vendor, for the seller's email), so each one can be re-checked.
    /// </summary>
    Task<IReadOnlyList<Dish>> GetByCatalogAndCountryAsync(
        Guid dishCatalogId, Country country, CancellationToken cancellationToken = default);

    // GetByIdAsync (inherited) includes the photos, the vendor (needed for CanBeOrdered)
    // and the DishCatalog WITH its PriceRanges (needed for ChangePrice).

    /// <summary>
    /// Food listing (Explore tab) + Home: only dishes a buyer can order right now:
    /// available, not waiting for a price update, from approved shops, not deleted.
    /// Filters match the app: country chip, category chip, search box, "View all" for one shop.
    /// Search matches the dish name or the shop name. Sorted as the app asks.
    /// </summary>
    Task<PagedResult<Dish>> SearchAsync(
        Country? country, DishCategory? category, string? search, Guid? vendorId,
        DishSort sort, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggestions: the dishes a buyer can order right now in one country (same rules as <see cref="SearchAsync"/>),
    /// with photos, shop and dish-list name (for its cuisine group), best rated first.
    /// </summary>
    /// <param name="country">The country the buyer is shopping in.</param>
    /// <param name="max">How many dishes at most.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<Dish>> GetOrderableInCountryAsync(
        Country country, int max, CancellationToken cancellationToken = default);
}