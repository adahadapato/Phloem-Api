namespace Phloem.Domain.Constants;

/// <summary>
/// Authorization policy names, used as [Authorize(Policy = Policies.RequireSeller)].
/// Each policy is registered in the Api project; the comment says which roles pass it.
/// Admin passes every policy.
/// </summary>
public static class Policies
{
    /// <summary>Admin only.</summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>Seller (approved shop) or Admin.</summary>
    public const string RequireSeller = "RequireSeller";

    /// <summary>CatalogManager or Admin.</summary>
    public const string RequireCatalogManager = "RequireCatalogManager";

    /// <summary>ShipmentManager or Admin.</summary>
    public const string RequireShipmentManager = "RequireShipmentManager";

    /// <summary>SupportAgent or Admin.</summary>
    public const string RequireSupport = "RequireSupport";
}