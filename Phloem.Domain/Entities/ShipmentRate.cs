using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// The price and typical transit time for one <see cref="DeliverySpeed"/> on one <see cref="ShipmentRoute"/>:
/// a base charge plus a price per chargeable kg (origin currency), and "usually 3–5 days".
/// </summary>
/// <remarks>Belongs to a ShipmentRoute and is only created / changed through it.</remarks>
public class ShipmentRate : Entity<Guid>
{
    /// <summary>Longest transit time an admin may enter (a guard against typos).</summary>
    public const int MaxTransitDaysLimit = 90;

    /// <summary>Required by EF Core.</summary>
    protected ShipmentRate() { }

    /// <summary>Creates the rate for one speed.</summary>
    /// <param name="shipmentRouteId">The route this rate belongs to.</param>
    /// <param name="speed">The speed.</param>
    /// <param name="baseCharge">Fixed amount per parcel; 0 or more.</param>
    /// <param name="pricePerKg">Amount per chargeable kg; 0 or more.</param>
    /// <param name="minTransitDays">Fewest days in transit; at least 1.</param>
    /// <param name="maxTransitDays">Most days in transit; at least <paramref name="minTransitDays"/>.</param>
    internal ShipmentRate(Guid shipmentRouteId, DeliverySpeed speed, decimal baseCharge, decimal pricePerKg,
                          int minTransitDays, int maxTransitDays)
        : base(Guid.CreateVersion7())
    {
        ShipmentRouteId = shipmentRouteId;
        Speed = speed;
        Set(baseCharge, pricePerKg, minTransitDays, maxTransitDays);
    }

    /// <summary>The route this rate belongs to.</summary>
    public Guid ShipmentRouteId { get; private set; }

    /// <summary>The speed this rate is for.</summary>
    public DeliverySpeed Speed { get; private set; }

    /// <summary>Fixed amount charged per parcel.</summary>
    public decimal BaseCharge { get; private set; }

    /// <summary>Amount charged per chargeable kg (see <see cref="ShipmentRoute.ChargeableWeight"/>).</summary>
    public decimal PricePerKg { get; private set; }

    /// <summary>Fewest days usually spent in transit (from leaving to arriving).</summary>
    public int MinTransitDays { get; private set; }

    /// <summary>Most days usually spent in transit.</summary>
    public int MaxTransitDays { get; private set; }

    /// <summary>Changes the price and transit time.</summary>
    internal void Update(decimal baseCharge, decimal pricePerKg, int minTransitDays, int maxTransitDays) =>
        Set(baseCharge, pricePerKg, minTransitDays, maxTransitDays);

    /// <exception cref="ArgumentOutOfRangeException">Negative amounts, or transit days outside 1–90 / max below min.</exception>
    private void Set(decimal baseCharge, decimal pricePerKg, int minTransitDays, int maxTransitDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseCharge);
        ArgumentOutOfRangeException.ThrowIfNegative(pricePerKg);
        ArgumentOutOfRangeException.ThrowIfLessThan(minTransitDays, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTransitDays, minTransitDays);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxTransitDays, MaxTransitDaysLimit);

        BaseCharge = decimal.Round(baseCharge, 2, MidpointRounding.AwayFromZero);
        PricePerKg = decimal.Round(pricePerKg, 2, MidpointRounding.AwayFromZero);
        MinTransitDays = minTransitDays;
        MaxTransitDays = maxTransitDays;
    }
}