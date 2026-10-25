using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IParcelItemRuleRepository"/>.</summary>
internal sealed class ParcelItemRuleRepository(PhloemDbContext context)
    : Repository<ParcelItemRule>(context), IParcelItemRuleRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ParcelItemRule>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Ordered(Set.AsNoTracking()).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ParcelItemRule>> GetAllForUpdateAsync(
        CancellationToken cancellationToken = default) =>
        await Ordered(Set).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<int> GetLastSortOrderAsync(CancellationToken cancellationToken = default) =>
        await Set.MaxAsync(r => (int?)r.SortOrder, cancellationToken) ?? -1;

    /// <summary>Lowest position first; the name breaks ties.</summary>
    private static IQueryable<ParcelItemRule> Ordered(IQueryable<ParcelItemRule> query) =>
        query.OrderBy(r => r.SortOrder).ThenBy(r => r.Name);
}
