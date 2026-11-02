using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Handles <see cref="ReinstateUserCommand"/>: lifts the suspension, brings the shop back, emails them.</summary>
public sealed class ReinstateUserCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IVendorRepository vendors,
    IAccountService accounts,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    ILogger<ReinstateUserCommandHandler> logger)
    : IRequestHandler<ReinstateUserCommand, Result>
{
    /// <returns>Success; or <see cref="UserProfileErrors.NotFound"/>.</returns>
    public async Task<Result> Handle(ReinstateUserCommand command, CancellationToken ct)
    {
        var profile = await profiles.GetByIdAsync(command.UserId, ct);
        // Not found, or someone in a country this person doesn't cover: answer the same way
        if (profile is null || !currentUser.CoversCountry(profile.Country)) return UserProfileErrors.NotFound;

        var wasSuspended = profile.SuspendedAtUtc is not null;
        profile.Reinstate();

        var vendor = await vendors.GetByOwnerIdAsync(command.UserId, ct);
        vendor?.SetOwnerSuspended(false);

        if (wasSuspended && currentUser.UserId is { } actorId)
            audit.Add(StaffAuditEntry.Create(actorId, StaffAction.UserReinstated,
                                             $"{profile.GetFullName()} ({command.UserId})", profile.Country));

        await unitOfWork.SaveChangesAsync(ct);
        if (!wasSuspended) return Result.Success();

        logger.LogInformation("User {UserId} reinstated by admin {AdminId}.", command.UserId, currentUser.UserId);
        await SuspensionEmails.SendLiftedAsync(accounts, email, logger, command.UserId, profile.FirstName, ct);
        return Result.Success();
    }
}

/// <summary>The "your suspension has been lifted" email (an admin lifting it, or the end date passing).</summary>
internal static class SuspensionEmails
{
    public static async Task SendLiftedAsync(IAccountService accounts, IEmailSender email, ILogger logger,
                                             Guid userId, string firstName, CancellationToken ct)
    {
        try
        {
            var contact = await accounts.GetContactAsync(userId, ct);
            if (contact.IsFailure) return;

            await email.SendAsync(contact.Value.Email, "Your Phloem account is active again",
                $"<p>Hello {WebUtility.HtmlEncode(firstName)},</p>" +
                "<p>Your Phloem account is no longer suspended: you can sign in again, and your shop " +
                "(if you have one) is back.</p>",
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email user {UserId} that their suspension was lifted.", userId);
        }
    }
}
