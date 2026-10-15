using Phloem.Domain.Entities;
using Phloem.Domain.Shared.Errors;

namespace Phloem.Domain.Errors;

/// <summary>
/// Every business-rule failure a Dish can return. Nothing here is thrown.
/// </summary>
public static class DishErrors
{
    // ---------- Creating / editing ----------

    public static readonly Error ShopBlocked = Error.Forbidden(
        "Dish.ShopBlocked", "This shop is blocked and can't add or change dishes.");

    public static readonly Error NameNotApproved = Error.Validation(
        "Dish.NameNotApproved", "Please choose a dish name from the approved list.");

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