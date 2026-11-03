using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>
/// Implements <see cref="IAiPromptProvider"/>: the newest saved wording, cached for 10 minutes; Phloem's default when
/// none is saved, or (not cached) when the database can't be read, so AI jobs keep working.
/// </summary>
internal sealed class AiPromptProvider(HybridCache cache, IServiceScopeFactory scopes, ILogger<AiPromptProvider> logger)
    : IAiPromptProvider
{
    private static readonly HybridCacheEntryOptions CacheFor = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };

    /// <inheritdoc />
    public async Task<AiPromptText> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var definition = AiPrompts.Find(key) ?? throw new ArgumentException($"Unknown AI prompt '{key}'.", nameof(key));

        try
        {
            return await cache.GetOrCreateAsync(CacheKey(key), async ct =>
            {
                await using var scope = scopes.CreateAsyncScope();
                var versions = scope.ServiceProvider.GetRequiredService<IAiPromptVersionRepository>();
                var latest = await versions.GetLatestAsync(key, ct);
                return latest is null
                    ? new AiPromptText(key, definition.DefaultPrompt, 0)
                    : new AiPromptText(key, latest.Text, latest.Version);
            }, CacheFor, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Couldn't read AI prompt {Key}; using Phloem's default wording.", key);
            return new AiPromptText(key, definition.DefaultPrompt, 0);
        }
    }

    /// <inheritdoc />
    public async Task ForgetAsync(string key, CancellationToken cancellationToken = default) =>
        await cache.RemoveAsync(CacheKey(key), cancellationToken);

    private static string CacheKey(string key) => $"ai:prompt:{key}";
}
