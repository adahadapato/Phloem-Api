namespace Phloem.Domain.Shared.Errors;


/// <summary>
/// Every business-rule failure a DishCatalog name can return. Nothing here is thrown.
/// </summary>
public static class DishCatalogErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "DishCatalog.NotFound", "That dish name wasn't found.");

    /// <summary>Two admins reviewing the same suggestion at once, or a starter-list name.</summary>
    public static readonly Error AlreadyReviewed = Error.Conflict(
        "DishCatalog.AlreadyReviewed", "This dish name has already been reviewed.");

    /// <summary>Prices can only be set for names sellers are allowed to use.</summary>
    public static readonly Error NotApproved = Error.Validation(
        "DishCatalog.NotApproved", "Price ranges can only be set for approved dish names.");

    /// <summary>"Can't find your dish?": the name is already on the list or already suggested.</summary>
    public static readonly Error NameTaken = Error.Conflict(
        "DishCatalog.NameTaken", "That dish name is already on the list or waiting for review.");
}