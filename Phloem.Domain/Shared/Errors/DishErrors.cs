using Phloem.Domain.Entities;
using Phloem.Domain.Shared.Errors;

namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure a Dish can return. Nothing here is thrown.
/// </summary>
public static class DishErrors
{
    // ---------- Creating / editing ----------

    public static readonly Error NotFound = Error.NotFound(
        "Dish.NotFound", "That dish wasn't found.");

    public static readonly Error ShopBlocked = Error.Forbidden(
        "Dish.ShopBlocked", "This shop is blocked and can't add or change dishes.");

    public static readonly Error NameNotApproved = Error.Validation(
        "Dish.NameNotApproved", "Please choose a dish name from the approved list.");

    // ---------- Price (admin-set range, inclusive) ----------

    /// <summary>No range for this dish in the seller's country yet, so it can't be sold there.</summary>
    public static readonly Error PriceNotSet = Error.Validation(
        "Dish.PriceNotSet",
        "Phloem hasn't set a price range for this dish in your country yet. Please try again later.");

    /// <summary>A method, because the message shows the allowed range.</summary>
    public static Error PriceOutOfRange(DishPriceRange range) => Error.Validation(
        "Dish.PriceOutOfRange",
        $"The price must be between {range.MinPrice:0.00} and {range.MaxPrice:0.00} {range.Currency}.");

    // ---------- Photos ----------

    public static readonly Error TooManyPhotos = Error.Validation(
        "Dish.TooManyPhotos", $"A dish can have at most {Dish.MaxPhotos} photos.");

    public static readonly Error DuplicatePhoto = Error.Conflict(
        "Dish.DuplicatePhoto", "You've already added this photo.");

    public static readonly Error PhotoNotFound = Error.NotFound(
        "Dish.PhotoNotFound", "That photo wasn't found on this dish.");

    /// <summary>The app sent an out-of-date photo list (e.g. a photo was added or removed in another tab).</summary>
    public static readonly Error PhotoOrderMismatch = Error.Validation(
        "Dish.PhotoOrderMismatch", "The photo list has changed. Please refresh and try again.");
}