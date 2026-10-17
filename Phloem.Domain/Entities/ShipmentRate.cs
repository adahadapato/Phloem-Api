using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// The price for one <see cref="DeliverySpeed"/> on one <see cref="ShipmentRoute"/>:
/// a base charge plus a price per chargeable kg, in the route's (origin country's) currency.
/// </summary>
/// <remarks>Belongs to a ShipmentRoute and is only created / changed through it.</remarks>
public class ShipmentRate : Entity<Guid>
{
    /// <summary>Required by EF Core.</summary>
    protected ShipmentRate() { }

    /// <summary>Creates the rate for one speed.</summary>
    /// <param name="shipmentRouteId">The route this rate belongs to.</param>
    /// <param name="speed">The speed.</param>
    /// <param name="baseCharge">Fixed amount per parcel; 0 or more.</param>
    /// <param name="pricePerKg">Amount per chargeable kg; 0 or more.</param>
    internal ShipmentRate(Guid shipmentRouteId, DeliverySpeed speed, decimal baseCharge, decimal pricePerKg)
        : base(Guid.CreateVersion7())
    {
        ShipmentRouteId = shipmentRouteId;
        Speed = speed;
        Set(baseCharge, pricePerKg);
    }

    /// <summary>The route this rate belongs to.</summary>
    public Guid ShipmentRouteId { get; private set; }

    /// <summary>The speed this rate is for.</summary>
    public DeliverySpeed Speed { get; private set; }

    /// <summary>Fixed amount charged per parcel.</summary>
    public decimal BaseCharge { get; private set; }

    /// <summary>Amount charged per chargeable kg (see <see cref="ShipmentRoute.ChargeableWeight"/>).</summary>
    public decimal PricePerKg { get; private set; }

    /// <summary>Changes the amounts.</summary>
    /// <param name="baseCharge">Fixed amount per parcel; 0 or more.</param>
    /// <param name="pricePerKg">Amount per chargeable kg; 0 or more.</param>
    internal void Update(decimal baseCharge, decimal pricePerKg) => Set(baseCharge, pricePerKg);

    /// <exception cref="ArgumentOutOfRangeException">A negative amount (the validator checks first).</exception>
    private void Set(decimal baseCharge, decimal pricePerKg)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseCharge);
        ArgumentOutOfRangeException.ThrowIfNegative(pricePerKg);

        BaseCharge = decimal.Round(baseCharge, 2, MidpointRounding.AwayFromZero);
        PricePerKg = decimal.Round(pricePerKg, 2, MidpointRounding.AwayFromZero);
    }
}