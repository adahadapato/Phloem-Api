using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// The extra amount a customer pays for one delivery window in one country,
/// e.g. Ghana, ASAP: +GHS 8.00. A window with no charge row is not offered in that country.
/// </summary>
/// <remarks>Belongs to a <see cref="DeliveryPricing"/> and is only created / changed through it.</remarks>
public class DeliveryWindowCharge : Entity<Guid>
{
    /// <summary>Required by EF Core.</summary>
    protected DeliveryWindowCharge() { }

    /// <summary>Creates the charge for one window.</summary>
    /// <param name="deliveryPricingId">The country's pricing this belongs to.</param>
    /// <param name="window">The delivery window.</param>
    /// <param name="extraCharge">Amount added to the shop's fee; 0 or more.</param>
    internal DeliveryWindowCharge(Guid deliveryPricingId, DeliveryWindow window, decimal extraCharge)
        : base(Guid.CreateVersion7())
    {
        DeliveryPricingId = deliveryPricingId;
        Window = window;
        Set(extraCharge);
    }

    /// <summary>The country's pricing this belongs to.</summary>
    public Guid DeliveryPricingId { get; private set; }

    /// <summary>The delivery window this charge is for.</summary>
    public DeliveryWindow Window { get; private set; }

    /// <summary>Amount added to the shop's delivery fee for this window (0 = no extra).</summary>
    public decimal ExtraCharge { get; private set; }

    /// <summary>Changes the extra charge.</summary>
    /// <param name="extraCharge">New amount; 0 or more.</param>
    internal void Update(decimal extraCharge) => Set(extraCharge);

    /// <exception cref="ArgumentOutOfRangeException">Negative amount (the validator checks first).</exception>
    private void Set(decimal extraCharge)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(extraCharge);
        ExtraCharge = decimal.Round(extraCharge, 2, MidpointRounding.AwayFromZero);
    }
}