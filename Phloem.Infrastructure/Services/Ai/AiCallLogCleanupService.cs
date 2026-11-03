using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;
using Phloem.Infrastructure.Settings.Ai;

namespace Phloem.Infrastructure.Services.Ai;

/// <summary>
/// Once a day, deletes AI calls older than <c>Ai:LogRetentionMonths</c> (default 12): the log holds people's text, so
/// it isn't kept for ever.
/// </summary>
/// <remarks>Same pattern as the other background tasks: a failed run is logged and tried again next time.</remarks>
internal sealed class AiCallLogCleanupService(
    IServiceScopeFactory scopes,
    IOptions<AiSettingsOptions> options,
    TimeProvider time,
    ILogger<AiCallLogCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(FirstRunDelay, stoppingToken);

            using var timer = new PeriodicTimer(Interval);
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The API is stopping
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            var cutoff = time.GetUtcNow().UtcDateTime.AddMonths(-options.Value.LogRetentionMonths);
            await using var scope = scopes.CreateAsyncScope();
            var deleted = await scope.ServiceProvider.GetRequiredService<IAiCallLogRepository>()
                                     .DeleteOlderThanAsync(cutoff, ct);
            if (deleted > 0) logger.LogInformation("Deleted {Count} AI calls older than {Cutoff:d}.", deleted, cutoff);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Deleting old AI calls failed; trying again in {Interval}.", Interval);
        }
    }
}
