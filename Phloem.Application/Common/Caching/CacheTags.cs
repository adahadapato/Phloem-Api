namespace Phloem.Application.Common.Caching;

/// <summary>One place for tag names, so a typo can't break cache clearing.</summary>
public static class CacheTags
{
    public const string DishCatalog = "dish-catalog";
    public const string Dishes = "dishes";
    public const string Vendors = "vendors";
    /// <summary>Delivery pricing per country (fee ranges and window charges).</summary>
    public const string DeliveryPricing = "delivery-pricing";
}