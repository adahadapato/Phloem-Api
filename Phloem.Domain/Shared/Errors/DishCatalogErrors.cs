using Phloem.Domain.Shared.Errors;

namespace Phloem.Domain.Errors;

/// <summary>
/// Every business-rule failure a DishCatalog name can return. Nothing here is thrown.
/// </summary>
public static class DishCatalogErrors
{
    /// <summary>Two admins reviewing the same suggestion at once, or a starter-list name.</summary>
    public static readonly Error AlreadyReviewed = Error.Conflict(
        "DishCatalog.AlreadyReviewed", "This dish name has already been reviewed.");
}