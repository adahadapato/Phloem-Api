namespace Phloem.Domain.Enums;

/// <summary>Food listing → "Sort by" (matches the app's options).</summary>
public enum DishSort
{
    Rating = 1,      // best rated first (default)
    PriceLow = 2,    // cheapest first
    PriceHigh = 3    // most expensive first
}