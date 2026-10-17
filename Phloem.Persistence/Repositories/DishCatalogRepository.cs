using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IDishCatalogRepository"/>.</summary>
/// <remarks>A catalog name is always loaded with its price ranges (the aggregate).</remarks>
internal sealed class DishCatalogRepository(PhloemDbContext context)
    : Repository<DishCatalog>(context), IDishCatalogRepository
{
    /// <inheritdoc />
    protected override IQueryable<DishCatalog> WithChildren(IQueryable<DishCatalog> query) =>
        query.Include(c => c.PriceRanges);

    /// <inheritdoc />
    /// <remarks>Read-only, with price ranges (the admin catalog page shows them).</remarks>
    public async Task<IReadOnlyList<DishCatalog>> GetApprovedAsync(CancellationToken cancellationToken = default) =>
        await WithChildren(Set.AsNoTracking())
                .Where(c => c.Status == DishCatalogStatus.Approved)
                .OrderBy(c => c.Group)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<DishCatalog>> GetSuggestionsAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
                 .Where(c => c.Status == DishCatalogStatus.Suggested)
                 .OrderBy(c => c.CreatedAt)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    /// <remarks>Matches the unique index exactly: Approved and Suggested names count, Rejected ones don't.</remarks>
    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var clean = name.Trim();
        return Set.AnyAsync(c => c.Name == clean && c.Status != DishCatalogStatus.Rejected, cancellationToken);
    }
}