using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="ReceiveShipmentAtHubCommand"/>.</summary>
public sealed class ReceiveShipmentAtHubCommandHandler(
    IShipmentRepository shipments,
    IShipmentRouteRepository routes,
    IAccountService accounts,
    IEmailSender email,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<ReceiveShipmentAtHubCommandHandler> logger)
    : IRequestHandler<ReceiveShipmentAtHubCommand, Result<decimal>>
{
    /// <summary>
    /// Weighs the parcel (the Domain sets the final price and transit days), saves, then emails the sender.
    /// A failed email is logged and never undoes the weighing.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls and email.</param>
    /// <returns>
    /// The final price; or <see cref="ShipmentErrors.NotFound"/>, <see cref="ShipmentErrors.RouteNotFound"/>,
    /// <see cref="ShipmentErrors.InvalidStatusChange"/>, <see cref="ShipmentErrors.SpeedNotOffered"/>.
    /// </returns>
    public async Task<Result<decimal>> Handle(ReceiveShipmentAtHubCommand command, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(command.TrackingNumber.Trim(), ct);
        if (shipment is null) return ShipmentErrors.NotFound;

        var route = await routes.GetByIdAsync(shipment.ShipmentRouteId, ct);
        if (route is null) return ShipmentErrors.RouteNotFound;

        var received = shipment.ReceiveAtHub(route, command.WeighedWeightKg, time.GetUtcNow().UtcDateTime);
        if (received.IsFailure) return received.Error;

        await unitOfWork.SaveChangesAsync(ct);

        await TryEmailSenderAsync(shipment, ct);
        return shipment.FinalPrice!.Value;
    }

    /// <summary>"Your parcel has been weighed: please pay X". Logs (doesn't throw) on failure.</summary>
    /// <param name="shipment">The weighed shipment.</param>
    /// <param name="ct">Cancels the lookup / send.</param>
    private async Task TryEmailSenderAsync(Shipment shipment, CancellationToken ct)
    {
        try
        {
            var contact = await accounts.GetContactAsync(shipment.SenderId, ct);
            if (contact.IsFailure) return;

            var body =
                $"<p>Hello {WebUtility.HtmlEncode(shipment.SenderName)},</p>" +
                $"<p>Your parcel <strong>{shipment.TrackingNumber}</strong> to " +
                $"{WebUtility.HtmlEncode(shipment.RecipientName)} has been received and weighed: " +
                $"<strong>{shipment.WeighedWeightKg:0.00} kg</strong>.</p>" +
                $"<p>The final price is <strong>{shipment.FinalPrice:0.00} {shipment.Currency}</strong>. " +
                "Please pay in the Phloem app (My shipments) so it can be sent.</p>";

            await email.SendAsync(contact.Value.Email, $"Your parcel {shipment.TrackingNumber} is ready to pay", body, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Weighed-parcel email for shipment {TrackingNumber} failed", shipment.TrackingNumber);
        }
    }
}
