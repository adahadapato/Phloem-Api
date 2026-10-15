using Phloem.Domain.Enums;
using Phloem.Domain.Errors;
using Phloem.Domain.Shared;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// "Send food to the UK": a parcel from Ghana or Nigeria to someone in the UK.
///
/// Errors: business-rule failures are RETURNED as a Result (see ShipmentErrors);
/// programmer mistakes (nulls, blank text the validator should have caught, a negative fee) still THROW.
/// </summary>
public class Shipment : Entity<Guid>
{
    protected Shipment() { }

    private Shipment(Guid id, string trackingNumber, Guid senderId, string senderName, string recipientName,
                     PhoneNumber? recipientPhone, Country fromCountry, string ukAddress, string itemDescription,
                     DeliverySpeed speed, decimal fee, Currency currency)
        : base(id)
    {
        TrackingNumber = trackingNumber;
        SenderId = senderId;
        SenderName = senderName;
        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        FromCountry = fromCountry;
        UkAddress = ukAddress;
        ItemDescription = itemDescription;
        Speed = speed;
        Fee = fee;
        Currency = currency;
        Status = ShipmentStatus.Processing;
    }

    public string TrackingNumber { get; private set; } = string.Empty;

    // ---------- Sender ----------
    public Guid SenderId { get; private set; }
    public virtual UserProfile Sender { get; protected set; } = null!;
    public string SenderName { get; private set; } = string.Empty;
    public Country FromCountry { get; private set; }               // Ghana or Nigeria

    // ---------- Recipient (UK) ----------
    public string RecipientName { get; private set; } = string.Empty;
    public PhoneNumber? RecipientPhone { get; private set; }
    public string UkAddress { get; private set; } = string.Empty;

    // ---------- Parcel ----------
    public string ItemDescription { get; private set; } = string.Empty;  // "Dried fish, gari, spices"
    public DeliverySpeed Speed { get; private set; }
    public decimal Fee { get; private set; }
    public Currency Currency { get; private set; }

    // ---------- Status ----------
    public ShipmentStatus Status { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>The fee is worked out by the Application layer from speed / weight.</summary>
    public static Result<Shipment> Create(string trackingNumber, UserProfile sender, string recipientName,
                                          PhoneNumber? recipientPhone, string ukAddress, string itemDescription,
                                          DeliverySpeed speed, decimal fee)
    {
        // Programmer mistakes → throw
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ukAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemDescription);
        ArgumentOutOfRangeException.ThrowIfNegative(fee);   // the Application layer calculates it

        // Business rules → return an error
        if (sender.IsDeleted) return ShipmentErrors.AccountDeleted;
        if (sender.Country == Country.UnitedKingdom) return ShipmentErrors.NotFromUk;

        return new Shipment(Guid.CreateVersion7(), trackingNumber.Trim(),
            sender.Id, sender.FullName,
            recipientName.Trim(), recipientPhone,
            sender.Country, ukAddress.Trim(),
            itemDescription.Trim(),
            speed, decimal.Round(fee, 2, MidpointRounding.AwayFromZero), sender.Country.ToCurrency());
    }

    /// <summary>ShipmentManager: the parcel has left.</summary>
    public Result MarkInTransit()
    {
        if (Status != ShipmentStatus.Processing)
            return ShipmentErrors.InvalidStatusChange(Status, ShipmentStatus.InTransit);

        Status = ShipmentStatus.InTransit;
        return Result.Success();
    }

    /// <summary>ShipmentManager: the parcel has arrived.</summary>
    public Result MarkDelivered(DateTime utcNow)
    {
        if (Status != ShipmentStatus.InTransit)
            return ShipmentErrors.InvalidStatusChange(Status, ShipmentStatus.Delivered);

        Status = ShipmentStatus.Delivered;
        DeliveredAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>Sender cancels before the parcel leaves. Cancelling twice is fine.</summary>
    public Result Cancel()
    {
        if (Status == ShipmentStatus.Cancelled) return Result.Success();
        if (Status != ShipmentStatus.Processing) return ShipmentErrors.CannotCancel;

        Status = ShipmentStatus.Cancelled;
        return Result.Success();
    }
}