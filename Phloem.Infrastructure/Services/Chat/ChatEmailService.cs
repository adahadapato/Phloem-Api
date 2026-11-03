using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Chat.Commands;

namespace Phloem.Infrastructure.Services.Chat;

/// <summary>
/// Every minute, emails people about chat messages they haven't seen for 5 minutes (<see cref="SendChatEmailsCommand"/>),
/// respecting their notification settings.
/// </summary>
/// <remarks>Same pattern as the other background tasks: a failed run is logged and tried again next time.</remarks>
internal sealed class ChatEmailService(IServiceScopeFactory scopes, ILogger<ChatEmailService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(1);

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
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(new SendChatEmailsCommand(), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Chat emails failed; trying again in {Interval}.", Interval);
        }
    }
}
