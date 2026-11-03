using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Moderation.Commands;

namespace Phloem.Infrastructure.Services.Moderation;

/// <summary>
/// Every minute, checks new text with the AI (<see cref="RunModerationCommand"/>): new and changed dishes (hidden
/// until checked), shop names, dish name suggestions and Contact messages. Up to 20 per run.
/// </summary>
/// <remarks>Same pattern as the other background tasks: a failed run is logged and tried again next time.</remarks>
internal sealed class ModerationWorker(IServiceScopeFactory scopes, ILogger<ModerationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(2);

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
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(new RunModerationCommand(), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Moderation checks failed; trying again in {Interval}.", Interval);
        }
    }
}
