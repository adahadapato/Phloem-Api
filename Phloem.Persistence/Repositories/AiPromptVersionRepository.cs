using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IAiPromptVersionRepository"/> with EF Core.</summary>
internal sealed class AiPromptVersionRepository(PhloemDbContext context)
    : Repository<AiPromptVersion>(context), IAiPromptVersionRepository
{
    /// <inheritdoc />
    public Task<AiPromptVersion?> GetLatestAsync(string key, CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
           .Where(v => v.Key == key)
           .OrderByDescending(v => v.Version)
           .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiPromptVersion>> GetLatestForAllAsync(CancellationToken cancellationToken = default)
    {
        // A handful of prompts with a few versions each: read them all and pick the newest per key here
        var all = await Set.AsNoTracking().ToListAsync(cancellationToken);
        return all.GroupBy(v => v.Key).Select(g => g.MaxBy(v => v.Version)!).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiPromptVersion>> GetVersionsAsync(string key,
                                                                       CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
                 .Where(v => v.Key == key)
                 .OrderByDescending(v => v.Version)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<AiPromptVersion?> GetVersionAsync(string key, int version, CancellationToken cancellationToken = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(v => v.Key == key && v.Version == version, cancellationToken);
}
