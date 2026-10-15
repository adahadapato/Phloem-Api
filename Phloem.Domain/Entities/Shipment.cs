using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using System.Diagnostics.Metrics;

namespace Phloem.Domain.Entities;

/// <summary>
/// "Send food to the UK": a parcel from Ghana or Nigeria to someone in the UK.
/// </summary>
public class Shipment : Entity<Guid>
{
    protected Shipment() { }

    private Shipment(Guid id, string trackingNumber, Guid senderId, string senderName, string recipientName,
                     string? recipientPhone, Country fromCountry, string ukAddress, string itemDescription,
                     DeliverySpeed speed, decimal fee, Currency currency, DateTime createdAtUtc)
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
        CreatedAtUtc = createdAtUtc;
    }

    public string TrackingNumber { get; private set; } = string.Empty;

    // ---------- Sender ----------
    public Guid SenderId { get; private set; }
    public virtual UserProfile Customer { get; protected set; } = null!;
    public string SenderName { get; private set; } = string.Empty;
    public Country FromCountry { get; private set; }               // Ghana or Nigeria

    // ---------- Recipient (UK) ----------
    public string RecipientName { get; private set; } = string.Empty;
    public string? RecipientPhone { get; private set; }
    public string UkAddress { get; private set; } = string.Empty;

    // ---------- Parcel ----------
    public string ItemDescription { get; private set; } = string.Empty;  // "Dried fish, gari, spices"
    public DeliverySpeed Speed { get; private set; }
    public decimal Fee { get; private set; }
    public Currency Currency { get; private set; }

    // ---------- Status ----------
    public ShipmentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>The fee is worked out by the Application layer from speed / weight.</summary>
    public static Shipment Create(string trackingNumber, UserProfile sender, string recipientName, string? recipientPhone,
                                  string ukAddress, string itemDescription, DeliverySpeed speed,
                                  decimal fee, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(sender);
        if (sender.Country == Country.UnitedKingdom)
            throw new InvalidOperationException("Shipments are sent from Ghana or Nigeria to the UK.");
        if (fee < 0) throw new ArgumentOutOfRangeException(nameof(fee), "Fee can't be negative.");

        return new Shipment(Guid.CreateVersion7(), Rules.Name(trackingNumber, "Tracking number", 30),
            sender.Id, sender.FullName,
            Rules.Name(recipientName, "Recipient name"), Rules.Phone(recipientPhone),
            sender.Country, Rules.Name(ukAddress, "UK address", 300),
            Rules.Name(itemDescription, "Item description", 300),
            speed, decimal.Round(fee, 2, MidpointRounding.AwayFromZero), sender.Country.ToCurrency(), utcNow);
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
