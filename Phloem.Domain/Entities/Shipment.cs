using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// "Send food to the UK": a parcel from Ghana or Nigeria to someone in the UK.
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
    public static Shipment Create(string trackingNumber, UserProfile sender, string recipientName, PhoneNumber? recipientPhone,
                                  string ukAddress, string itemDescription, DeliverySpeed speed,
                                  decimal fee)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ukAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemDescription);
        if (sender.Country == Country.UnitedKingdom)
            throw new InvalidOperationException("Shipments are sent from Ghana or Nigeria to the UK.");
        if (fee < 0) throw new ArgumentOutOfRangeException(nameof(fee), "Fee can't be negative.");

        return new Shipment(Guid.CreateVersion7(), trackingNumber.Trim(),
            sender.Id, sender.FullName,
            recipientName.Trim(), recipientPhone,
            sender.Country, ukAddress.Trim(),
            itemDescription.Trim(),
            speed, decimal.Round(fee, 2, MidpointRounding.AwayFromZero), sender.Country.ToCurrency());
    }

    public void MarkInTransit()
    {
        if (Status != ShipmentStatus.Processing)
            throw new InvalidOperationException($"A shipment that is {Status} can't go in transit.");
        Status = ShipmentStatus.InTransit;
    }

    public void MarkDelivered(DateTime utcNow)
    {
        if (Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException($"A shipment that is {Status} can't be delivered.");
        Status = ShipmentStatus.Delivered;
        DeliveredAtUtc = utcNow;
    }

    public void Cancel()
    {
        if (Status != ShipmentStatus.Processing)
            throw new InvalidOperationException("Only shipments that haven't left yet can be cancelled.");
        Status = ShipmentStatus.Cancelled;
    }
}