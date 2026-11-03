using Phloem.Domain.Entities;

namespace Phloem.Domain.Interfaces;

/// <summary>Search engine text per website page.</summary>
public interface ISeoEntryRepository : IRepository<SeoEntry>
{
    /// <summary>One page's entry (tracked, to change it), or null.</summary>
    /// <param name="key">The page key.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<SeoEntry?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Every entry, by key (read-only). A few thousand rows at most: one per page.</summary>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyDictionary<string, SeoEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}
