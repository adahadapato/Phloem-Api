using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IDishRepository"/>.</summary>
/// <remarks>
/// <para><b>Whole aggregate</b> (<see cref="GetByIdAsync"/>): photos, the shop (needed for <c>CanBeOrdered</c>) and the
/// catalog name WITH its price ranges (needed for <c>ChangePrice</c>).</para>
/// <para>Deleted dishes, and dishes of closed shops, are hidden by the soft-delete filters.</para>
/// </remarks>
internal sealed class DishRepository(PhloemDbContext context)
    : Repository<Dish>(context), IDishRepository
{
    /// <inheritdoc />
    protected override IQueryable<Dish> WithChildren(IQueryable<Dish> query) =>
        query.Include(d => d.Photos)
             .Include(d => d.Vendor)
             .Include(d => d.DishCatalog).ThenInclude(c => c.PriceRanges);

    /// <inheritdoc />
    /// <remarks>
    /// Only what a buyer can order right now: available, not waiting for a price update, from an approved shop
    /// whose owner isn't suspended.
    /// Read-only, with photos and shop (the dish cards show both).
    /// </remarks>
    public Task<PagedResult<Dish>> SearchAsync(Country? country, DishCategory? category, string? search,
                                                Guid? vendorId, DishSort sort, PageRequest page,
                                                CancellationToken cancellationToken = default)
    {
        IQueryable<Dish> query = Set.AsNoTracking()
            .Include(d => d.Photos)
            .Include(d => d.Vendor)
            .Where(d => d.IsAvailable
                     && !d.NeedsPriceUpdate
                     && d.Vendor.Status == VendorStatus.Approved
                     && !d.Vendor.IsOwnerSuspended);

        if (country is not null) query = query.Where(d => d.Country == country);
        if (category is not null) query = query.Where(d => d.Category == category);
        if (vendorId is not null) query = query.Where(d => d.VendorId == vendorId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.Name.Contains(term) || d.Vendor.Name.Contains(term));
        }

        var ordered = sort switch
        {
            DishSort.PriceLow => query.OrderBy(d => d.Price).ThenBy(d => d.Name),
            DishSort.PriceHigh => query.OrderByDescending(d => d.Price).ThenBy(d => d.Name),
            _ => query.OrderByDescending(d => d.Rating).ThenByDescending(d => d.RatingCount).ThenBy(d => d.Name)
        };

        return ToPagedAsync(ordered, page, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>Older overload without a sort: best rated first. Kept until it's removed from the interface.</remarks>
    public Task<PagedResult<Dish>> SearchAsync(Country? country, DishCategory? category, string? search,
                                                Guid? vendorId, PageRequest page,
                                                CancellationToken cancellationToken = default) =>
        SearchAsync(country, category, search, vendorId, DishSort.Rating, page, cancellationToken);

    /// <inheritdoc />
    /// <remarks>Read-only; every dish of the shop that isn't deleted, by name.</remarks>
    public async Task<IReadOnlyList<Dish>> GetByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
                 .Include(d => d.Photos)
                 .Include(d => d.DishCatalog).ThenInclude(c => c.PriceRanges)
                 .Where(d => d.VendorId == vendorId)
                 .OrderBy(d => d.Name)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    /// <remarks>Tracked, with shop and photos (order snapshots use both).</remarks>
    public async Task<IReadOnlyList<Dish>> GetByIdsAsync(IReadOnlyCollection<Guid> dishIds,
                                                         CancellationToken cancellationToken = default)
    {
        if (dishIds.Count == 0) return [];

        return await Set.Include(d => d.Vendor)
                        .Include(d => d.Photos)
                        .Where(d => dishIds.Contains(d.Id))
                        .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, int>> CountByVendorsAsync(IReadOnlyCollection<Guid> vendorIds,
                                                                          CancellationToken cancellationToken = default)
    {
        if (vendorIds.Count == 0) return new Dictionary<Guid, int>();

        return await Set.AsNoTracking()
                        .Where(d => vendorIds.Contains(d.VendorId))
                        .GroupBy(d => d.VendorId)
                        .Select(g => new { VendorId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(g => g.VendorId, g => g.Count, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>Tracked: the caller re-checks each dish against the new range and saves.</remarks>
    public async Task<IReadOnlyList<Dish>> GetByCatalogAndCountryAsync(Guid dishCatalogId, Country country,
                                                                       CancellationToken cancellationToken = default) =>
        await Set.Include(d => d.Vendor)
                 .Where(d => d.DishCatalogId == dishCatalogId && d.Country == country)
                 .ToListAsync(cancellationToken);
}