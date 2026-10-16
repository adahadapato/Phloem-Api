using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Entities;

/// <summary>
/// The admin's delivery pricing for ONE country: the range a shop's own delivery fee must sit in,
/// and the extra charge for each delivery window. One per country.
/// </summary>
/// <remarks>
/// <para><b>What the customer pays</b> = the shop's fee (<see cref="Vendor.DeliveryFee"/>, or this
/// country's <see cref="MinShopFee"/> if the shop hasn't set one) + the extra for the chosen
/// <see cref="DeliveryWindow"/>. Always calculated on the server with <see cref="FeeFor"/>.</para>
/// <para><b>Shop fee range</b> (inclusive) lets shops in remote areas charge more, without anyone
/// charging silly amounts. When the range changes, shops outside it are moved to the nearest
/// limit (<see cref="Vendor.ApplyDeliveryFeeRange"/>) and emailed. Shops are never blocked.</para>
/// <para><b>Windows</b>: a window with no <see cref="DeliveryWindowCharge"/> is not offered in
/// this country.</para>
/// <para>Bad amounts (negative, max below min) are checked by validators first, so they THROW here.</para>
/// </remarks>
public class DeliveryPricing : Entity<Guid>
{
    private readonly List<DeliveryWindowCharge> _windowCharges = new();

    /// <summary>Required by EF Core.</summary>
    protected DeliveryPricing() { }

    private DeliveryPricing(Guid id, Country country, decimal minShopFee, decimal maxShopFee)
        : base(id)
    {
        Country = country;
        SetRange(minShopFee, maxShopFee);
    }

    /// <summary>The country this pricing is for (unique).</summary>
    public Country Country { get; private set; }

    /// <summary>The country's currency; all amounts are in it.</summary>
    public Currency Currency => Country.ToCurrency();

    /// <summary>Lowest delivery fee a shop may charge (inclusive). Also the fee of shops that haven't set one.</summary>
    public decimal MinShopFee { get; private set; }

    /// <summary>Highest delivery fee a shop may charge (inclusive).</summary>
    public decimal MaxShopFee { get; private set; }

    /// <summary>Extra charge per offered delivery window.</summary>
    public virtual IReadOnlyCollection<DeliveryWindowCharge> WindowCharges => _windowCharges.AsReadOnly();

    // =====================================================================
    // Admin
    // =====================================================================

    /// <summary>Creates the pricing for a country (admin, or seeding).</summary>
    /// <param name="country">The country; must not already have pricing (the Application layer checks).</param>
    /// <param name="minShopFee">Lowest shop fee; 0 or more.</param>
    /// <param name="maxShopFee">Highest shop fee; at least <paramref name="minShopFee"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Negative min, or max below min.</exception>
    public static DeliveryPricing Create(Country country, decimal minShopFee, decimal maxShopFee) =>
        new(Guid.CreateVersion7(), country, minShopFee, maxShopFee);

    /// <summary>
    /// Changes the shop fee range. Afterwards, call <see cref="Vendor.ApplyDeliveryFeeRange"/> for
    /// every shop in this country so their fees stay inside it.
    /// </summary>
    /// <param name="minShopFee">Lowest shop fee; 0 or more.</param>
    /// <param name="maxShopFee">Highest shop fee; at least <paramref name="minShopFee"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Negative min, or max below min.</exception>
    public void SetShopFeeRange(decimal minShopFee, decimal maxShopFee) => SetRange(minShopFee, maxShopFee);

    /// <summary>Offers a delivery window in this country (or changes its extra charge).</summary>
    /// <param name="window">The window.</param>
    /// <param name="extraCharge">Amount added to the shop's fee; 0 or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">Negative amount.</exception>
    public void SetWindowCharge(DeliveryWindow window, decimal extraCharge)
    {
        var existing = _windowCharges.FirstOrDefault(c => c.Window == window);
        if (existing is null) _windowCharges.Add(new DeliveryWindowCharge(Id, window, extraCharge));
        else existing.Update(extraCharge);
    }

    /// <summary>Stops offering a delivery window in this country. Removing one that isn't offered is fine.</summary>
    /// <param name="window">The window.</param>
    public void RemoveWindow(DeliveryWindow window)
    {
        var existing = _windowCharges.FirstOrDefault(c => c.Window == window);
        if (existing is not null) _windowCharges.Remove(existing);
    }

    // =====================================================================
    // Using it
    // =====================================================================

    /// <summary>Is this window offered in this country?</summary>
    /// <param name="window">The window.</param>
    public bool Offers(DeliveryWindow window) => _windowCharges.Any(c => c.Window == window);

    /// <summary>True if a shop fee is inside the range (inclusive).</summary>
    /// <param name="fee">The fee to check.</param>
    public bool ContainsShopFee(decimal fee) => fee >= MinShopFee && fee <= MaxShopFee;

    /// <summary>The nearest allowed shop fee: below the range → min; above → max; inside → unchanged.</summary>
    /// <param name="fee">The fee to adjust.</param>
    public decimal ClampShopFee(decimal fee) => Math.Clamp(fee, MinShopFee, MaxShopFee);

    /// <summary>
    /// What the customer pays for delivery from this shop in this window.
    /// </summary>
    /// <param name="vendor">The shop (must be in this country).</param>
    /// <param name="window">The window the customer chose.</param>
    /// <returns>The fee; or <see cref="DeliveryPricingErrors.WindowNotOffered"/>.</returns>
    /// <exception cref="InvalidOperationException">The shop is in a different country (a bug).</exception>
    public Result<decimal> FeeFor(Vendor vendor, DeliveryWindow window)
    {
        ArgumentNullException.ThrowIfNull(vendor);
        if (vendor.Country != Country)
            throw new InvalidOperationException($"This pricing is for {Country}, not {vendor.Country}.");

        var charge = _windowCharges.FirstOrDefault(c => c.Window == window);
        if (charge is null) return DeliveryPricingErrors.WindowNotOffered(window);

        var shopFee = vendor.DeliveryFee ?? MinShopFee;
        return shopFee + charge.ExtraCharge;
    }

    /// <exception cref="ArgumentOutOfRangeException">Negative min, or max below min.</exception>
    private void SetRange(decimal minShopFee, decimal maxShopFee)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minShopFee);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxShopFee, minShopFee);

        MinShopFee = decimal.Round(minShopFee, 2, MidpointRounding.AwayFromZero);
        MaxShopFee = decimal.Round(maxShopFee, 2, MidpointRounding.AwayFromZero);
    }
}