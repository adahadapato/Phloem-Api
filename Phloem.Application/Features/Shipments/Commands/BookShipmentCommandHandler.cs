using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="BookShipmentCommand"/>.</summary>
/// <remarks>The sender must have their date of birth and home address on their profile, and be 18 or older
/// (<see cref="UserProfile.CheckCanTrade"/>).</remarks>
public sealed class BookShipmentCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IShipmentRouteRepository routes,
    IShipmentRepository shipments,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<BookShipmentCommand, Result<BookedShipmentDto>>
{
    /// <summary>How many times to try for an unused tracking number before giving up (a bug if reached).</summary>
    private const int MaxTrackingNumberAttempts = 10;

    /// <summary>
    /// 1. Load the sender and check they can send parcels. 2. Load the route. 3. Make a tracking number.
    /// 4. <see cref="Shipment.Book"/> (all rules in Domain). 5. Save.
    /// </summary>
    /// <param name="command">The booking form.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>
    /// The booking; or <see cref="UserProfileErrors.NotFound"/>, <see cref="UserProfileErrors.PersonalDetailsRequired"/>,
    /// <see cref="UserProfileErrors.TooYoung"/>, <see cref="ShipmentErrors.RouteNotAvailable"/>,
    /// or any booking error from <see cref="Shipment.Book"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user, or no free tracking number.</exception>
    public async Task<Result<BookedShipmentDto>> Handle(BookShipmentCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var now = time.GetUtcNow().UtcDateTime;                              // NEW: moved up

        var sender = await profiles.GetByIdAsync(userId, ct);
        if (sender is null) return UserProfileErrors.NotFound;

        // NEW: date of birth + home address filled in, and 18 or older
        var canTrade = sender.CheckCanTrade(DateOnly.FromDateTime(now));
        if (canTrade.IsFailure) return canTrade.Error;

        var route = await routes.GetByCountriesAsync(command.FromCountry, command.ToCountry, ct);
        if (route is null) return ShipmentErrors.RouteNotAvailable;

        var trackingNumber = await NewTrackingNumberAsync(now, ct);

        var recipient = new ShipmentRecipient(
            command.RecipientName,
            PhoneNumber.Create(command.RecipientPhone),
            command.RecipientAddressLine,
            command.RecipientCity,
            command.RecipientRegionOrPostcode);

        var items = command.Items.Select(i =>
            new ShipmentItemDetails(i.Category, i.Description, i.Quantity, i.UnitDeclaredValue));

        var booked = Shipment.Book(trackingNumber, sender, PhoneNumber.Create(command.SenderPhone), route,
                                   recipient, items, command.Speed, command.EstimatedWeightKg,
                                   command.ConfirmedNoProhibitedItems, now);
        if (booked.IsFailure) return booked.Error;

        shipments.Add(booked.Value);
        await unitOfWork.SaveChangesAsync(ct);

        return BookedShipmentDto.From(booked.Value, route);
    }

    /// <summary>
    /// "PHS-yyMMdd-######" (date + 6 random digits), unused in the database.
    /// The database's unique index is the final guard.
    /// </summary>
    /// <param name="utcNow">The current time (for the date part).</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>An unused tracking number.</returns>
    /// <exception cref="InvalidOperationException">No free number after several tries.</exception>
    private async Task<string> NewTrackingNumberAsync(DateTime utcNow, CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxTrackingNumberAttempts; attempt++)
        {
            var number = $"PHS-{utcNow:yyMMdd}-{Random.Shared.Next(100_000, 1_000_000)}";
            if (!await shipments.TrackingNumberExistsAsync(number, ct)) return number;
        }

        throw new InvalidOperationException("Couldn't generate a unique tracking number.");
    }
}