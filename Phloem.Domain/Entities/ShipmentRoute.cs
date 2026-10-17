using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Entities;

/// <summary>
/// One shipping direction set up by an admin, e.g. Ghana → United Kingdom, with its drop-off hub,
/// an on/off switch, and a price per <see cref="DeliverySpeed"/>.
/// </summary>
/// <remarks>
/// <para><b>Routes are data:</b> adding Ghana ⇄ Nigeria later is just a new route, no code change.
/// UK → Ghana and Ghana → UK are two separate routes (prices and hubs differ).</para>
/// <para><b>Price</b> = base charge + price per kg × chargeable weight, where chargeable weight is the
/// weight ROUNDED UP to the next <see cref="WeightStepKg"/> (7.2 kg → 7.5 kg). All amounts are in the
/// ORIGIN country's currency, because the sender pays where the parcel is dropped off.</para>
/// <para>A new route starts switched OFF, so the admin can add rates before customers see it.
/// A speed with no rate isn't offered on the route.</para>
/// <para>Bad input (same country twice, blank hub address, negative amounts) is checked by validators
/// first, so it THROWS here.</para>
/// </remarks>
public class ShipmentRoute : Entity<Guid>
{
    /// <summary>Weights are rounded UP to a multiple of this before pricing (courier practice).</summary>
    public const decimal WeightStepKg = 0.5m;

    /// <summary>Longest drop-off hub address.</summary>
    public const int DropOffAddressMaxLength = 300;

    /// <summary>Longest opening-hours text, e.g. "Mon–Sat, 9am–6pm".</summary>
    public const int DropOffHoursMaxLength = 200;

    private readonly List<ShipmentRate> _rates = new();

    /// <summary>Required by EF Core.</summary>
    protected ShipmentRoute() { }

    private ShipmentRoute(Guid id, Country fromCountry, Country toCountry, string dropOffAddress, string dropOffHours)
        : base(id)
    {
        FromCountry = fromCountry;
        ToCountry = toCountry;
        IsActive = false;
        SetDropOff(dropOffAddress, dropOffHours);
    }

    /// <summary>Where parcels are sent FROM (and where the sender drops them off and pays).</summary>
    public Country FromCountry { get; private set; }

    /// <summary>Where parcels are sent TO.</summary>
    public Country ToCountry { get; private set; }

    /// <summary>The origin country's currency; all prices are in it.</summary>
    public Currency Currency => FromCountry.ToCurrency();

    /// <summary>The hub address where senders drop parcels off (shown in the app after booking).</summary>
    public string DropOffAddress { get; private set; } = string.Empty;

    /// <summary>The hub's opening hours, e.g. "Mon–Sat, 9am–6pm".</summary>
    public string DropOffHours { get; private set; } = string.Empty;

    /// <summary>True = customers can book on this route.</summary>
    public bool IsActive { get; private set; }

    /// <summary>The price for each offered speed.</summary>
    public virtual IReadOnlyCollection<ShipmentRate> Rates => _rates.AsReadOnly();

    // =====================================================================
    // Admin
    // =====================================================================

    /// <summary>Creates a route, switched OFF until the admin activates it.</summary>
    /// <param name="fromCountry">Origin country.</param>
    /// <param name="toCountry">Destination country; must differ from the origin.</param>
    /// <param name="dropOffAddress">The origin hub's address.</param>
    /// <param name="dropOffHours">The origin hub's opening hours.</param>
    /// <exception cref="ArgumentException">Same country twice, or blank text.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Text too long.</exception>
    public static ShipmentRoute Create(Country fromCountry, Country toCountry, string dropOffAddress, string dropOffHours)
    {
        if (fromCountry == toCountry)
            throw new ArgumentException("A route must go between two different countries.", nameof(toCountry));

        return new ShipmentRoute(Guid.CreateVersion7(), fromCountry, toCountry, dropOffAddress, dropOffHours);
    }

    /// <summary>Changes the drop-off hub details.</summary>
    /// <param name="dropOffAddress">The hub's address.</param>
    /// <param name="dropOffHours">The hub's opening hours.</param>
    /// <exception cref="ArgumentException">Blank text.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Text too long.</exception>
    public void UpdateDropOff(string dropOffAddress, string dropOffHours) => SetDropOff(dropOffAddress, dropOffHours);

    /// <summary>Sets (or changes) the price and transit time for one speed.</summary>
    /// <param name="speed">The speed.</param>
    /// <param name="baseCharge">Fixed amount per parcel; 0 or more.</param>
    /// <param name="pricePerKg">Amount per chargeable kg; 0 or more.</param>
    /// <param name="minTransitDays">Fewest days in transit; at least 1.</param>
    /// <param name="maxTransitDays">Most days in transit; at least the minimum, at most 90.</param>
    /// <exception cref="ArgumentOutOfRangeException">Bad amounts or days (the validator checks first).</exception>
    public void SetRate(DeliverySpeed speed, decimal baseCharge, decimal pricePerKg, int minTransitDays, int maxTransitDays)
    {
        var existing = _rates.FirstOrDefault(r => r.Speed == speed);
        if (existing is null) _rates.Add(new ShipmentRate(Id, speed, baseCharge, pricePerKg, minTransitDays, maxTransitDays));
        else existing.Update(baseCharge, pricePerKg, minTransitDays, maxTransitDays);
    }

    /// <summary>Stops offering a speed on this route. Removing one that isn't offered is fine.</summary>
    /// <param name="speed">The speed.</param>
    public void RemoveRate(DeliverySpeed speed)
    {
        var existing = _rates.FirstOrDefault(r => r.Speed == speed);
        if (existing is not null) _rates.Remove(existing);
    }

    /// <summary>Switches the route on so customers can book.</summary>
    /// <returns>Success; or <see cref="ShipmentErrors.RouteHasNoRates"/> if no speed has a price yet.</returns>
    public Result Activate()
    {
        if (_rates.Count == 0) return ShipmentErrors.RouteHasNoRates;
        IsActive = true;
        return Result.Success();
    }

    /// <summary>Switches the route off. Existing shipments carry on; no new bookings.</summary>
    public void Deactivate() => IsActive = false;

    // =====================================================================
    // Pricing
    // =====================================================================

    /// <summary>Is this speed offered on this route?</summary>
    /// <param name="speed">The speed.</param>
    public bool Offers(DeliverySpeed speed) => _rates.Any(r => r.Speed == speed);

    /// <summary>The weight that is charged: rounded UP to the next <see cref="WeightStepKg"/>.</summary>
    /// <param name="weightKg">The (estimated or weighed) weight in kg.</param>
    public static decimal ChargeableWeight(decimal weightKg) =>
        Math.Ceiling(weightKg / WeightStepKg) * WeightStepKg;

    /// <summary>The price for a parcel of this weight at this speed.</summary>
    /// <param name="speed">The chosen speed.</param>
    /// <param name="weightKg">Weight in kg; more than 0.</param>
    /// <returns>The price; or <see cref="ShipmentErrors.RouteNotAvailable"/>, <see cref="ShipmentErrors.SpeedNotOffered"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Weight 0 or less (the validator checks first).</exception>
    public Result<decimal> PriceFor(DeliverySpeed speed, decimal weightKg)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weightKg);

        if (!IsActive) return ShipmentErrors.RouteNotAvailable;

        var rate = _rates.FirstOrDefault(r => r.Speed == speed);
        if (rate is null) return ShipmentErrors.SpeedNotOffered(speed);

        var price = rate.BaseCharge + rate.PricePerKg * ChargeableWeight(weightKg);
        return decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }

    /// <exception cref="ArgumentException">Blank text.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Text too long.</exception>
    private void SetDropOff(string dropOffAddress, string dropOffHours)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dropOffAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(dropOffHours);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dropOffAddress.Trim().Length, DropOffAddressMaxLength, nameof(dropOffAddress));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dropOffHours.Trim().Length, DropOffHoursMaxLength, nameof(dropOffHours));

        DropOffAddress = dropOffAddress.Trim();
        DropOffHours = dropOffHours.Trim();
    }
}