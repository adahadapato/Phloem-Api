using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// "Send a parcel" → Book. The sender is the signed-in user (never taken from the form).
/// The price is an ESTIMATE; the final price is set when the parcel is weighed at the hub.
/// </summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="SenderPhone">The sender's contact phone for this parcel.</param>
/// <param name="RecipientName">Recipient's full name.</param>
/// <param name="RecipientPhone">Recipient's phone.</param>
/// <param name="RecipientAddressLine">Street and house number.</param>
/// <param name="RecipientCity">City or town.</param>
/// <param name="RecipientRegionOrPostcode">Region (Ghana / Nigeria) or postcode (UK).</param>
/// <param name="Items">What's in the parcel (at least one).</param>
/// <param name="Speed">The chosen speed.</param>
/// <param name="EstimatedWeightKg">The sender's weight estimate.</param>
/// <param name="ConfirmedNoProhibitedItems">"My parcel contains no prohibited items" ticked.</param>
/// <param name="AcceptedTerms">"I accept the shipping terms" ticked.</param>
public sealed record BookShipmentCommand(
    Country FromCountry,
    Country ToCountry,
    string SenderPhone,
    string RecipientName,
    string RecipientPhone,
    string RecipientAddressLine,
    string RecipientCity,
    string RecipientRegionOrPostcode,
    IReadOnlyList<ShipmentItemInputDto> Items,
    DeliverySpeed Speed,
    decimal EstimatedWeightKg,
    bool ConfirmedNoProhibitedItems,
    bool AcceptedTerms) : IRequest<Result<BookedShipmentDto>>;
