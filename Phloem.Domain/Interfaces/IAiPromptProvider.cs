using Phloem.Domain.Models;

namespace Phloem.Domain.Interfaces;

/// <summary>The wording each AI job uses now (cached for 10 minutes; Phloem's default if the database fails).</summary>
public interface IAiPromptProvider
{
    /// <summary>The current wording of a job's prompt.</summary>
    /// <param name="key">The prompt key (from <see cref="Constants.AiPrompts"/>).</param>
    /// <param name="cancellationToken">Stops the lookup.</param>
    /// <returns>The wording and its version (0 = default).</returns>
    /// <exception cref="ArgumentException">Unknown key (a programming mistake).</exception>
    Task<AiPromptText> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Drops the cached wording after a save, so the next call uses the new one at once.</summary>
    /// <param name="key">The prompt key.</param>
    /// <param name="cancellationToken">Stops the cache call.</param>
    Task ForgetAsync(string key, CancellationToken cancellationToken = default);
}
