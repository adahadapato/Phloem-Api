using Phloem.Domain.Enums;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// The shape of <c>starter-data.json</c>: the first values for a new database. Admins change them in the app
/// afterwards; the seeder never overwrites.
/// </summary>
public sealed class StarterData
{
    /// <summary>Phloem's commission per country.</summary>
    public List<CommissionSeed> Commissions { get; set; } = [];

    /// <summary>Delivery fee range and window charges per country.</summary>
    public List<DeliveryPricingSeed> DeliveryPricing { get; set; } = [];

    /// <summary>The dish names sellers pick from, with price ranges per country.</summary>
    public List<DishSeed> Dishes { get; set; } = [];

    /// <summary>"Send food" routes (seeded switched off, without rates).</summary>
    public List<RouteSeed> ShipmentRoutes { get; set; } = [];

    /// <summary>The "what can be sent" list, in display order.</summary>
    public List<ParcelItemRuleSeed> ParcelItemRules { get; set; } = [];
}

/// <summary>One country's commission.</summary>
public sealed class CommissionSeed
{
    /// <summary>The country.</summary>
    public Country Country { get; set; }

    /// <summary>Percentage of each order, e.g. 10 = 10%.</summary>
    public decimal RatePercent { get; set; }
}

/// <summary>One country's delivery pricing.</summary>
public sealed class DeliveryPricingSeed
{
    /// <summary>The country.</summary>
    public Country Country { get; set; }

    /// <summary>Lowest delivery fee a shop may charge.</summary>
    public decimal MinShopFee { get; set; }

    /// <summary>Highest delivery fee a shop may charge.</summary>
    public decimal MaxShopFee { get; set; }

    /// <summary>Offered delivery windows and their extra charge (a window not listed isn't offered).</summary>
    public List<WindowChargeSeed> Windows { get; set; } = [];
}

/// <summary>One offered delivery window.</summary>
public sealed class WindowChargeSeed
{
    /// <summary>The window.</summary>
    public DeliveryWindow Window { get; set; }

    /// <summary>Extra charge on top of the shop's delivery fee (0 = none).</summary>
    public decimal ExtraCharge { get; set; }
}

/// <summary>One dish name.</summary>
public sealed class DishSeed
{
    /// <summary>e.g. "Jollof Rice".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Meal, snack or drink.</summary>
    public DishCategory Category { get; set; }

    /// <summary>Group in the seller's dish picker.</summary>
    public CatalogGroup Group { get; set; }

    /// <summary>Allowed price per country (a country not listed can't sell it until an admin adds a range).</summary>
    public List<PriceRangeSeed> PriceRanges { get; set; } = [];
}

/// <summary>One country's price range for a dish.</summary>
public sealed class PriceRangeSeed
{
    /// <summary>The country.</summary>
    public Country Country { get; set; }

    /// <summary>Lowest price.</summary>
    public decimal Min { get; set; }

    /// <summary>Highest price.</summary>
    public decimal Max { get; set; }
}

/// <summary>One shipping route.</summary>
public sealed class RouteSeed
{
    /// <summary>Origin.</summary>
    public Country From { get; set; }

    /// <summary>Destination.</summary>
    public Country To { get; set; }

    /// <summary>Where senders drop parcels off.</summary>
    public string DropOffAddress { get; set; } = string.Empty;

    /// <summary>When the drop-off point is open.</summary>
    public string DropOffHours { get; set; } = string.Empty;
}

/// <summary>One line of the "what can be sent" list.</summary>
public sealed class ParcelItemRuleSeed
{
    /// <summary>NotAllowed or Restricted.</summary>
    public ParcelItemRuleKind Kind { get; set; }

    /// <summary>e.g. "Fresh meat or fish".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional condition or reason.</summary>
    public string? Detail { get; set; }
}
