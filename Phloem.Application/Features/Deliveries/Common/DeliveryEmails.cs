using System.Net;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Deliveries.Common;

/// <summary>
/// The delivery emails: "You have a new delivery" to the courier, and "Delivered" to the customer or parcel sender
/// (only if their order / parcel updates are switched on). Emails never fail the request: errors are logged.
/// </summary>
public sealed class DeliveryEmails(
    IAccountService accounts,
    IUserProfileRepository profiles,
    IEmailSender email,
    ILogger<DeliveryEmails> logger)
{
    /// <summary>Tells a courier they've been given a delivery.</summary>
    /// <param name="courierId">The courier.</param>
    /// <param name="reference">The order or tracking number.</param>
    /// <param name="what">e.g. "A food order from Mama Put" or "A parcel".</param>
    /// <param name="address">Where it's going.</param>
    /// <param name="ct">Cancels sending.</param>
    public async Task NewDeliveryAsync(Guid courierId, string reference, string what, string address,
                                       CancellationToken ct)
    {
        try
        {
            var contact = await accounts.GetContactAsync(courierId, ct);
            if (contact.IsFailure) return;

            await email.SendAsync(contact.Value.Email, $"New delivery: {reference}",
                $"<p>{WebUtility.HtmlEncode(what)} (<b>{WebUtility.HtmlEncode(reference)}</b>) is yours to deliver to:</p>" +
                $"<blockquote>{WebUtility.HtmlEncode(address)}</blockquote>" +
                "<p>Open <b>My deliveries</b> in the Phloem app for the details. When you hand it over, tap " +
                "<b>Delivered</b> and take a photo of where you left it.</p>",
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email courier {CourierId} about {Reference}.", courierId, reference);
        }
    }

    /// <summary>Tells the customer their food order was delivered (if they get order updates).</summary>
    /// <param name="order">The delivered order (with its proof).</param>
    /// <param name="ct">Cancels sending.</param>
    public Task OrderDeliveredAsync(Order order, CancellationToken ct) =>
        DeliveredAsync(order.CustomerId, wantsIt: p => p.NotifyOrderUpdates,
                       subject: $"Delivered: your order from {order.VendorName}",
                       what: $"Your order <b>{WebUtility.HtmlEncode(order.OrderNumber)}</b> from " +
                             $"<b>{WebUtility.HtmlEncode(order.VendorName)}</b>",
                       summary: order.DeliveryProof?.Describe(), reference: order.OrderNumber, ct);

    /// <summary>Tells the sender their parcel was delivered (if they get parcel updates).</summary>
    /// <param name="shipment">The delivered parcel (with its proof).</param>
    /// <param name="ct">Cancels sending.</param>
    public Task ParcelDeliveredAsync(Shipment shipment, CancellationToken ct) =>
        DeliveredAsync(shipment.SenderId, wantsIt: p => p.NotifyShipmentUpdates,
                       subject: $"Delivered: parcel {shipment.TrackingNumber}",
                       what: $"Your parcel <b>{WebUtility.HtmlEncode(shipment.TrackingNumber)}</b> to " +
                             $"<b>{WebUtility.HtmlEncode(shipment.RecipientName)}</b>",
                       summary: shipment.DeliveryProof?.Describe(), reference: shipment.TrackingNumber, ct);

    private async Task DeliveredAsync(Guid userId, Func<UserProfile, bool> wantsIt, string subject, string what,
                                      string? summary, string reference, CancellationToken ct)
    {
        try
        {
            var profile = await profiles.GetByIdAsync(userId, ct);
            if (profile is null || !wantsIt(profile)) return;

            var contact = await accounts.GetContactAsync(userId, ct);
            if (contact.IsFailure) return;

            var how = summary is null ? "" : $" {WebUtility.HtmlEncode(summary)}.";
            await email.SendAsync(contact.Value.Email, subject,
                $"<p>{what} has been delivered.{how}</p>" +
                "<p>The courier took a photo when they handed it over: you'll find it on the order page in the " +
                "Phloem app or website.</p>" +
                "<p>Something not right? Just reply to this email.</p>",
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email {UserId} that {Reference} was delivered.", userId, reference);
        }
    }
}
