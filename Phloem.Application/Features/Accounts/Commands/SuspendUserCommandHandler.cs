using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Handles <see cref="SuspendUserCommand"/>: suspends the person, hides their shop and cancels its unfinished
/// orders, signs them out everywhere, saves it all at once, then emails them (and the buyers whose orders were
/// cancelled). Emails never fail the request.
/// </summary>
public sealed class SuspendUserCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IUserRoleService roles,
    IVendorRepository vendors,
    IOrderRepository orders,
    IRefreshTokenRepository refreshTokens,
    IAccountService accounts,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    TimeProvider time,
    ILogger<SuspendUserCommandHandler> logger)
    : IRequestHandler<SuspendUserCommand, Result<UserSuspendedDto>>
{
    /// <returns>What it did; or <see cref="UserProfileErrors.NotFound"/>, <see cref="UserProfileErrors.CannotSuspendAdmin"/>,
    /// <see cref="UserProfileErrors.CannotSuspendYourself"/>, <see cref="UserProfileErrors.SuspensionEndInPast"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in admin (endpoint misconfigured).</exception>
    public async Task<Result<UserSuspendedDto>> Handle(SuspendUserCommand command, CancellationToken ct)
    {
        var adminId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in admin.");

        var profile = await profiles.GetByIdAsync(command.UserId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        if ((await roles.GetRolesAsync(command.UserId, ct)).Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase))
            return UserProfileErrors.CannotSuspendAdmin;

        // 1. The person
        var now = time.GetUtcNow().UtcDateTime;
        var suspended = profile.Suspend(command.Reason, command.UntilUtc, adminId, now);
        if (suspended.IsFailure) return suspended.Error;

        // 2. Their shop: hidden with them; orders that haven't left it are cancelled
        var vendor = await vendors.GetByOwnerIdAsync(command.UserId, ct);
        var cancelled = new List<Order>();
        if (vendor is not null)
        {
            vendor.SetOwnerSuspended(true);
            foreach (var order in await orders.GetUnfinishedForVendorAsync(vendor.Id, ct))
                if (order.CancelBecauseShopSuspended(now)) cancelled.Add(order);
        }

        // 3. Signed out everywhere
        foreach (var token in await refreshTokens.GetActiveForUserAsync(command.UserId, now, ct))
            token.Revoke(now);

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogWarning("User {UserId} suspended by admin {AdminId} until {Until}: {Reason}. " +
                          "Shop orders cancelled: {Cancelled}.",
                          command.UserId, adminId, command.UntilUtc?.ToString("u") ?? "further notice",
                          command.Reason.Trim(), cancelled.Count);

        // 4. Emails (never fail the request)
        await TellThePersonAsync(command.UserId, profile.FirstName, profile.SuspendedUntilUtc,
                                 profile.SuspensionReason!, ct);
        foreach (var order in cancelled) await TellTheBuyerAsync(order, ct);

        return new UserSuspendedDto(vendor is not null, cancelled.Count, cancelled.Count(o => o.NeedsRefund));
    }

    private async Task TellThePersonAsync(Guid userId, string firstName, DateTime? untilUtc, string reason,
                                          CancellationToken ct)
    {
        try
        {
            var contact = await accounts.GetContactAsync(userId, ct);
            if (contact.IsFailure) return;

            var until = untilUtc is { } u ? $"until <b>{u:d MMMM yyyy}</b>" : "until further notice";
            await email.SendAsync(contact.Value.Email, "Your Phloem account has been suspended",
                $"<p>Hello {WebUtility.HtmlEncode(firstName)},</p>" +
                $"<p>Your Phloem account has been suspended {until}, for this reason:</p>" +
                $"<blockquote>{WebUtility.HtmlEncode(reason)}</blockquote>" +
                "<p>While it's suspended you can't sign in, and your shop (if you have one) is hidden.</p>" +
                "<p>If you think this is a mistake, just reply to this email.</p>",
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email user {UserId} about their suspension.", userId);
        }
    }

    private async Task TellTheBuyerAsync(Order order, CancellationToken ct)
    {
        try
        {
            var contact = await accounts.GetContactAsync(order.CustomerId, ct);
            if (contact.IsFailure) return;

            var refund = order.NeedsRefund ? "<p>You'll get your money back in full.</p>" : string.Empty;
            await email.SendAsync(contact.Value.Email, $"Your order {order.OrderNumber} was cancelled",
                $"<p>Sorry, <b>{WebUtility.HtmlEncode(order.VendorName)}</b> can't take orders right now, so we've " +
                $"cancelled your order <b>{WebUtility.HtmlEncode(order.OrderNumber)}</b>.</p>{refund}" +
                "<p>You'll find other great shops in the app.</p>",
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email the buyer of {OrderNumber} about its cancellation.", order.OrderNumber);
        }
    }
}
