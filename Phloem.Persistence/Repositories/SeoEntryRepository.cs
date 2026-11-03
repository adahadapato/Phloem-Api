using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="ISeoEntryRepository"/> with EF Core.</summary>
internal sealed class SeoEntryRepository(PhloemDbContext context)
    : Repository<SeoEntry>(context), ISeoEntryRepository
{
    /// <inheritdoc />
    public Task<SeoEntry?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(e => e.Key == key, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, SeoEntry>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().ToDictionaryAsync(e => e.Key, cancellationToken);
}
