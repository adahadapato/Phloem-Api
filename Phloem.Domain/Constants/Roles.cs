namespace Phloem.Domain.Constants;

/// <summary>
/// Identity role names. Seeded at startup; use these constants instead of typing strings.
/// </summary>
public static class Roles
{
    /// <summary>Everything: approve sellers, block shops, manage users and staff.</summary>
    public const string Admin = "Admin";

    /// <summary>Every signed-up user gets this.</summary>
    public const string Customer = "Customer";

    /// <summary>Given when an admin APPROVES the user's shop; removed if the shop is blocked or closed.</summary>
    public const string Seller = "Seller";

    /// <summary>Approves or rejects dish-name suggestions and edits the dish list.</summary>
    public const string CatalogManager = "CatalogManager";

    /// <summary>Handles "Send food to the UK": moves shipments to In transit / Delivered.</summary>
    public const string ShipmentManager = "ShipmentManager";

    /// <summary>Customer support: can look up orders and users, cancel orders for customers.</summary>
    public const string SupportAgent = "SupportAgent";

    /// <summary>All roles, for seeding.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        Admin, Customer, Seller, CatalogManager, ShipmentManager, SupportAgent
    ];
}