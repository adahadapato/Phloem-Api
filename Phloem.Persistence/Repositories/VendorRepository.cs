using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IVendorRepository"/>.</summary>
/// <remarks>
/// <para>A shop is loaded with its documents (the aggregate) wherever it may be changed.</para>
/// <para>Closed shops are soft-deleted, so the global filter leaves them out of every list. Only
/// <see cref="NameExistsAsync"/> counts them, because their names stay taken (unique index).</para>
/// <para>Name comparisons rely on SQL Server's default case-insensitive collation.</para>
/// </remarks>
internal sealed class VendorRepository(PhloemDbContext context)
    : Repository<Vendor>(context), IVendorRepository
{
    /// <inheritdoc />
    protected override IQueryable<Vendor> WithChildren(IQueryable<Vendor> query) =>
        query.Include(v => v.Documents);

    /// <inheritdoc />
    public Task<Vendor?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        WithChildren(Set).FirstOrDefaultAsync(v => v.OwnerId == ownerId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> NameExistsAsync(string name, Guid? exceptVendorId = null,
                                      CancellationToken cancellationToken = default)
    {
        var clean = name.Trim();
        return Set.IgnoreQueryFilters()
                  .AnyAsync(v => v.Name == clean && (exceptVendorId == null || v.Id != exceptVendorId),
                            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Vendor>> GetPendingApprovalAsync(CancellationToken cancellationToken = default) =>
        await WithChildren(Set.AsNoTracking())
                .Where(v => v.Status == VendorStatus.PendingApproval)
                .OrderBy(v => v.CreatedAt)
                .ToListAsync(cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// Read-only. Order: verified shops first, then best rated, then name, so buyers see trusted shops first;
    /// admins get the same stable order.
    /// </remarks>
    public Task<PagedResult<Vendor>> GetPagedAsync(VendorStatus? status, Country? country, string? search,
                                                    PageRequest page, CancellationToken cancellationToken = default,
                                                    bool visibleToBuyersOnly = false)
    {
        IQueryable<Vendor> query = Set.AsNoTracking();

        // Buyers: approved shops whose owner isn't suspended (same rule as Vendor.IsVisibleToBuyers)
        if (visibleToBuyersOnly) query = query.Where(v => v.Status == VendorStatus.Approved && !v.IsOwnerSuspended);
        if (status is not null) query = query.Where(v => v.Status == status);
        if (country is not null) query = query.Where(v => v.Country == country);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(v => v.Name.Contains(term));
        }

        var ordered = query.OrderByDescending(v => v.IsVerified)
                           .ThenByDescending(v => v.Rating)
                           .ThenBy(v => v.Name);

        return ToPagedAsync(ordered, page, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>Shops buyers can see (approved, owner not suspended); ties broken by number of ratings, then name.</remarks>
    public async Task<IReadOnlyList<Vendor>> GetTopRatedAsync(Country? country, int count,
                                                              CancellationToken cancellationToken = default)
    {
        IQueryable<Vendor> query = Set.AsNoTracking()
                                      .Where(v => v.Status == VendorStatus.Approved && !v.IsOwnerSuspended);
        if (country is not null) query = query.Where(v => v.Country == country);

        return await query.OrderByDescending(v => v.Rating)
                          .ThenByDescending(v => v.RatingCount)
                          .ThenBy(v => v.Name)
                          .Take(count)
                          .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<VendorStatus, int>> CountByStatusAsync(
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
                 .GroupBy(v => v.Status)
                 .Select(g => new { Status = g.Key, Count = g.Count() })
                 .ToDictionaryAsync(g => g.Status, g => g.Count, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesByOwnerIdsAsync(
        IReadOnlyCollection<Guid> ownerIds, CancellationToken cancellationToken = default)
    {
        if (ownerIds.Count == 0) return new Dictionary<Guid, string>();

        return await Set.AsNoTracking()
                        .Where(v => ownerIds.Contains(v.OwnerId))
                        .Select(v => new { v.OwnerId, v.Name })
                        .ToDictionaryAsync(v => v.OwnerId, v => v.Name, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>Tracked: the caller changes each shop's fee and saves.</remarks>
    public async Task<IReadOnlyList<Vendor>> GetByCountryAsync(Country country,
                                                               CancellationToken cancellationToken = default) =>
        await Set.Where(v => v.Country == country).ToListAsync(cancellationToken);
}