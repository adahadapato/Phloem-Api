using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Handles <see cref="LiftEndedSuspensionsCommand"/>: reinstates everyone whose end date has passed.</summary>
public sealed class LiftEndedSuspensionsCommandHandler(
    IUserProfileRepository profiles,
    IVendorRepository vendors,
    IAccountService accounts,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    TimeProvider time,
    ILogger<LiftEndedSuspensionsCommandHandler> logger)
    : IRequestHandler<LiftEndedSuspensionsCommand, Result<int>>
{
    /// <returns>How many suspensions were lifted.</returns>
    public async Task<Result<int>> Handle(LiftEndedSuspensionsCommand command, CancellationToken ct)
    {
        var ended = await profiles.GetEndedSuspensionsAsync(time.GetUtcNow().UtcDateTime, ct);
        if (ended.Count == 0) return 0;

        foreach (var profile in ended)
        {
            profile.Reinstate();
            var vendor = await vendors.GetByOwnerIdAsync(profile.Id, ct);
            vendor?.SetOwnerSuspended(false);
        }

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Lifted {Count} suspension(s) whose end date had passed.", ended.Count);

        foreach (var profile in ended)
            await SuspensionEmails.SendLiftedAsync(accounts, email, logger, profile.Id, profile.FirstName, ct);

        return ended.Count;
    }
}
