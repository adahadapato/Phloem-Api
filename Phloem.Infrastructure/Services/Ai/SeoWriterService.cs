using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Seo.Commands;

namespace Phloem.Infrastructure.Services.Ai;

/// <summary>
/// Every 30 minutes, the AI writes search engine text (SEO) for pages that need it
/// (<see cref="WriteMissingSeoCommand"/>): new shops, shops that changed, and main pages never written. Up to 25 pages
/// per run, so a new Phloem with many shops catches up over a few hours without a big bill.
/// </summary>
/// <remarks>When AI is off or this month's cap is reached, the run simply writes nothing. Staff wording is never
/// touched. A failed run is logged and tried again next time.</remarks>
internal sealed class SeoWriterService(IServiceScopeFactory scopes, ILogger<SeoWriterService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(5);
    private const int PagesPerRun = 25;

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
            var result = await scope.ServiceProvider.GetRequiredService<ISender>()
                                    .Send(new WriteMissingSeoCommand(PagesPerRun), ct);
            if (result.IsSuccess && result.Value > 0)
                logger.LogInformation("The AI wrote search engine text for {Count} pages.", result.Value);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Writing search engine text failed; trying again in {Interval}.", Interval);
        }
    }
}
