using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Approvals.Commands;

namespace Phloem.Infrastructure.Services.Approvals;

/// <summary>
/// Every 15 minutes, expires changes nobody decided in time (<see cref="ExpirePendingChangesCommand"/>): a shop
/// block nobody confirmed within 24 hours is undone, and a Terms proposal nobody approved within 7 days lapses.
/// </summary>
/// <remarks>Same pattern as the suspension task: a failed run is logged and tried again next time; it never stops
/// the API.</remarks>
internal sealed class ApprovalExpiryService(IServiceScopeFactory scopes, ILogger<ApprovalExpiryService> logger)
    : BackgroundService
{
    /// <summary>How often it checks (a block's 24 hours ends at most this late).</summary>
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    /// <summary>Wait after start-up, so the database is migrated and seeded first.</summary>
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(3);

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
            await using var scope = scopes.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ExpirePendingChangesCommand(), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Expiring changes nobody decided failed; trying again in {Interval}.", Interval);
        }
    }
}
