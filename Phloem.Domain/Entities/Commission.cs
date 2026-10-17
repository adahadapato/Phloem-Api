using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// The percentage Phloem keeps from the DISHES of each delivered order in one country
/// (never from the delivery fee, which always goes to the seller). One per country.
/// </summary>
/// <remarks>
/// A country with no Commission row is treated as 0%. When an order is delivered, the amount is
/// worked out and STORED on the order (<see cref="Order.CommissionAmount"/>), so changing the rate
/// later never changes orders already delivered. Bad rates are checked by the validator first,
/// so they THROW here.
/// </remarks>
public class Commission : Entity<Guid>
{
    /// <summary>Highest rate an admin may set (a guard against typos like 100 instead of 10).</summary>
    public const decimal MaxRatePercent = 50m;

    /// <summary>Required by EF Core.</summary>
    protected Commission() { }

    private Commission(Guid id, Country country, decimal ratePercent)
        : base(id)
    {
        Country = country;
        ChangeRate(ratePercent);
    }

    /// <summary>The country this rate applies to (unique).</summary>
    public Country Country { get; private set; }

    /// <summary>The rate, e.g. 10 = 10% of the dishes' subtotal.</summary>
    public decimal RatePercent { get; private set; }

    /// <summary>Creates the rate for a country (admin).</summary>
    /// <param name="country">The country; must not already have a rate (the Application layer checks).</param>
    /// <param name="ratePercent">0 to <see cref="MaxRatePercent"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Rate below 0 or above the maximum.</exception>
    public static Commission Create(Country country, decimal ratePercent) =>
        new(Guid.CreateVersion7(), country, ratePercent);

    /// <summary>Changes the rate. Only affects orders delivered from now on.</summary>
    /// <param name="ratePercent">0 to <see cref="MaxRatePercent"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Rate below 0 or above the maximum.</exception>
    public void ChangeRate(decimal ratePercent)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ratePercent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ratePercent, MaxRatePercent);
        RatePercent = decimal.Round(ratePercent, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>The commission on a dishes subtotal, rounded to 2 decimal places.</summary>
    /// <param name="dishesSubtotal">The order's dishes subtotal (delivery fee NOT included).</param>
    public decimal AmountOn(decimal dishesSubtotal) =>
        decimal.Round(dishesSubtotal * RatePercent / 100m, 2, MidpointRounding.AwayFromZero);
}