using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="DeclineOrderCommand"/> for the signed-in seller's shop. Emails never fail the request.</summary>
public sealed class DeclineOrderCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    IAccountService accounts,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    TimeProvider time,
    ILogger<DeclineOrderCommandHandler> logger)
    : IRequestHandler<DeclineOrderCommand, Result>
{
    /// <summary>The team is emailed when a shop declines MORE than this many orders in <see cref="AlertPeriod"/>.</summary>
    public const int DeclinesBeforeTeamAlert = 3;

    /// <summary>The period declines are counted over.</summary>
    public static readonly TimeSpan AlertPeriod = TimeSpan.FromDays(7);

    /// <returns>Success; or <see cref="VendorErrors.NotFound"/>, <see cref="OrderErrors.NotFound"/>,
    /// <see cref="OrderErrors.CannotDecline"/>.</returns>
    public async Task<Result> Handle(DeclineOrderCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        if (order is null || order.VendorId != vendor.Id || order.Status == OrderStatus.AwaitingPayment)
            return OrderErrors.NotFound;

        var now = time.GetUtcNow().UtcDateTime;
        var declined = order.DeclineByShop(command.Reason, now);
        if (declined.IsFailure) return declined;

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Shop {Shop} declined order {OrderNumber}: {Reason}",
                              vendor.Name, order.OrderNumber, order.DeclineReason);

        await TellTheBuyerAsync(order, ct);

        var recent = await orders.CountDeclinedForVendorSinceAsync(vendor.Id, now - AlertPeriod, ct);
        if (recent == DeclinesBeforeTeamAlert + 1)   // once, when it passes the limit
            await AlertTheTeamAsync(vendor, recent, ct);

        return Result.Success();
    }

    private async Task TellTheBuyerAsync(Order order, CancellationToken ct)
    {
        try
        {
            var contact = await accounts.GetContactAsync(order.CustomerId, ct);
            if (contact.IsFailure) return;

            await email.SendAsync(contact.Value.Email, $"{order.VendorName} couldn't take your order",
                $"<p>Sorry, <b>{WebUtility.HtmlEncode(order.VendorName)}</b> couldn't make your order " +
                $"<b>{WebUtility.HtmlEncode(order.OrderNumber)}</b>:</p>" +
                $"<blockquote>{WebUtility.HtmlEncode(order.DeclineReason)}</blockquote>" +
                "<p>The order is cancelled and you'll get your money back in full. " +
                "You'll find other great shops in the app.</p>",
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email the buyer of {OrderNumber} that it was declined.", order.OrderNumber);
        }
    }

    private async Task AlertTheTeamAsync(Vendor vendor, int declines, CancellationToken ct)
    {
        try
        {
            await email.SendToTeamAsync($"{vendor.Name} has declined {declines} orders this week",
                $"<p><b>{WebUtility.HtmlEncode(vendor.Name)}</b> ({vendor.Country.DisplayName()}) has declined " +
                $"{declines} orders in the last {AlertPeriod.TotalDays:0} days.</p>" +
                "<p>Have a look in Admin → Sellers.</p>", ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't alert the team about {Shop}'s declines.", vendor.Name);
        }
    }
}
