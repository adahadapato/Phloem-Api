using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Models;

/// <summary>
/// Step 3 of the "Send a parcel" form: who receives the parcel and where. An INPUT for
/// <see cref="Shipment.Book"/>; the values are copied onto the shipment. The country is the route's
/// destination, so it isn't asked for.
/// </summary>
/// <param name="FullName">The recipient's full name.</param>
/// <param name="Phone">The recipient's phone (the courier calls it).</param>
/// <param name="AddressLine">Street and house number.</param>
/// <param name="City">City or town.</param>
/// <param name="RegionOrPostcode">Region (Ghana / Nigeria) or postcode (UK).</param>
public sealed record ShipmentRecipient(
    string FullName,
    PhoneNumber Phone,
    string AddressLine,
    string City,
    string RegionOrPostcode);