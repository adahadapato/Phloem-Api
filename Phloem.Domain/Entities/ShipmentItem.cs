using Phloem.Domain.Enums;
using Phloem.Domain.Models;

namespace Phloem.Domain.Entities;

/// <summary>One item in a <see cref="Shipment"/>, declared by the sender for customs.</summary>
/// <remarks>Only created through its Shipment; never changed after booking (customs record).</remarks>
public class ShipmentItem : Entity<Guid>
{
    /// <summary>Longest item description.</summary>
    public const int DescriptionMaxLength = 200;

    /// <summary>Most of one item (a guard against typos).</summary>
    public const int MaxQuantity = 999;

    /// <summary>Required by EF Core.</summary>
    protected ShipmentItem() { }

    /// <summary>Creates the item from the form's details.</summary>
    /// <param name="shipmentId">The shipment this item belongs to.</param>
    /// <param name="details">What the sender entered.</param>
    /// <exception cref="ArgumentException">Blank description.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Description too long, quantity outside 1–999, or a negative value.</exception>
    internal ShipmentItem(Guid shipmentId, ShipmentItemDetails details)
        : base(Guid.CreateVersion7())
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(details.Description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(details.Description.Trim().Length, DescriptionMaxLength, nameof(details));
        ArgumentOutOfRangeException.ThrowIfLessThan(details.Quantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(details.Quantity, MaxQuantity);
        ArgumentOutOfRangeException.ThrowIfNegative(details.UnitDeclaredValue);

        ShipmentId = shipmentId;
        Category = details.Category;
        Description = details.Description.Trim();
        Quantity = details.Quantity;
        UnitDeclaredValue = decimal.Round(details.UnitDeclaredValue, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>The shipment this item belongs to.</summary>
    public Guid ShipmentId { get; private set; }

    /// <summary>What kind of item.</summary>
    public ShipmentItemCategory Category { get; private set; }

    /// <summary>The sender's description, e.g. "Gari, 2 kg bags".</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>How many.</summary>
    public int Quantity { get; private set; }

    /// <summary>Declared value of ONE item, in the shipment's currency.</summary>
    public decimal UnitDeclaredValue { get; private set; }

    /// <summary><see cref="UnitDeclaredValue"/> × <see cref="Quantity"/>.</summary>
    public decimal TotalDeclaredValue => UnitDeclaredValue * Quantity;
}