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
    /// Seller dashboard → "My dishes": ALL of the shop's dishes, including ones switched off
    /// with "Available today", so the seller can switch them back on.
    /// </summary>
    Task<IReadOnlyList<Dish>> GetByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Place order / Buy again: the basket's dishes in one query (with vendor, for the snapshot).
    /// Missing or deleted Ids are simply not returned, so the caller can say "no longer available".
    /// </summary>
    Task<IReadOnlyList<Dish>> GetByIdsAsync(
        IReadOnlyCollection<Guid> dishIds, CancellationToken cancellationToken = default);
}