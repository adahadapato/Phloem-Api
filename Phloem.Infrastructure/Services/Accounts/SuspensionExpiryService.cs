using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Accounts.Commands;

namespace Phloem.Infrastructure.Services.Accounts;

/// <summary>
/// Every hour, lifts suspensions whose end date has passed (<see cref="LiftEndedSuspensionsCommand"/>), so people can
/// sign in again and their shops come back without an admin doing anything. Also runs once shortly after start-up.
/// </summary>
/// <remarks>
/// Sign-in already ignores a suspension whose end date has passed (UserProfile.IsSuspendedAt); this task tidies the
/// record and brings the shop back. A failed run is logged and tried again next hour; it never stops the API.
/// </remarks>
internal sealed class SuspensionExpiryService(IServiceScopeFactory scopes, ILogger<SuspensionExpiryService> logger)
    : BackgroundService
{
    /// <summary>How often it checks.</summary>
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    /// <summary>Wait after start-up, so the database is migrated and seeded first.</summary>
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(2);

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
            await sender.Send(new LiftEndedSuspensionsCommand(), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Lifting ended suspensions failed; trying again in {Interval}.", Interval);
        }
    }
}
