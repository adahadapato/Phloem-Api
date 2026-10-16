using Phloem.Domain.Entities;
using Phloem.Domain.Enums;


namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure about delivery pricing. Nothing here is thrown.
/// </summary>
public static class DeliveryPricingErrors
{
    /// <summary>The admin hasn't set up delivery pricing for this country yet, so orders can't be delivered there.</summary>
    public static readonly Error NotSet = Error.Conflict(
        "DeliveryPricing.NotSet", "Delivery isn't available in this country yet.");

    /// <summary>Creating pricing for a country that already has it.</summary>
    public static readonly Error AlreadyExists = Error.Conflict(
        "DeliveryPricing.AlreadyExists", "Delivery pricing for this country already exists.");

    /// <summary>The customer chose a delivery window this country doesn't offer.</summary>
    /// <param name="window">The window they chose.</param>
    public static Error WindowNotOffered(DeliveryWindow window) => Error.Validation(
        "DeliveryPricing.WindowNotOffered", $"\"{window}\" delivery isn't offered here. Please choose another time.");

    /// <summary>A seller set a delivery fee outside the admin's range.</summary>
    /// <param name="pricing">The country's pricing, for the allowed range in the message.</param>
    public static Error ShopFeeOutOfRange(DeliveryPricing pricing) => Error.Validation(
        "DeliveryPricing.ShopFeeOutOfRange",
        $"Your delivery fee must be between {pricing.MinShopFee:0.00} and {pricing.MaxShopFee:0.00} {pricing.Currency}.");
}