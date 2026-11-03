using Phloem.Domain.Entities;

namespace Phloem.Domain.Interfaces;

/// <summary>Saved wordings of AI prompts. Versions are only ever added.</summary>
public interface IAiPromptVersionRepository : IRepository<AiPromptVersion>
{
    /// <summary>The version in use for a key (the highest), or null when the default is in use.</summary>
    /// <param name="key">The prompt key.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<AiPromptVersion?> GetLatestAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>The version in use for every key that has one (read-only).</summary>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<AiPromptVersion>> GetLatestForAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Every version of a key, newest first (read-only).</summary>
    /// <param name="key">The prompt key.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<AiPromptVersion>> GetVersionsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>One version of a key, or null.</summary>
    /// <param name="key">The prompt key.</param>
    /// <param name="version">The version number.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<AiPromptVersion?> GetVersionAsync(string key, int version, CancellationToken cancellationToken = default);
}
