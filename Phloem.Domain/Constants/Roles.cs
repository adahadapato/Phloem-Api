namespace Phloem.Domain.Constants;

/// <summary>
/// Identity role names. Seeded at startup; use these constants instead of typing strings.
/// </summary>
/// <remarks>
/// <para><b>Three kinds of role:</b></para>
/// <list type="bullet">
///   <item><description><b>Admin</b>: the owners. Every permission in every country.</description></item>
///   <item><description><b>Job roles</b> (<see cref="JobRoles"/>): Phloem's staff. Each is a named set of
///   <see cref="Permissions"/> (role claims, seeded from <see cref="JobRolePresets"/>), and each staff member also has
///   the countries they cover (<c>StaffCountry</c>).</description></item>
///   <item><description><b>Customer, Seller, Courier</b>: everyone else. No staff permissions.</description></item>
/// </list>
/// <para><b>Stored names never change.</b> The website and app already use "SupportAgent", "CatalogManager" and
/// "ShipmentManager", so those job roles keep their stored names; people see them as "Support", "Shops &amp;
/// catalogue" and "Parcel hub" (<see cref="DisplayName"/>).</para>
/// </remarks>
public static class Roles
{
    /// <summary>The owners: every permission in every country.</summary>
    public const string Admin = "Admin";

    /// <summary>Every signed-up user gets this.</summary>
    public const string Customer = "Customer";

    /// <summary>Given when an admin APPROVES the user's shop; removed if the shop is blocked or closed.</summary>
    public const string Seller = "Seller";

    /// <summary>Job role "Shops &amp; catalogue": approves shops and their documents, edits the dish list.</summary>
    public const string CatalogManager = "CatalogManager";

    /// <summary>Job role "Parcel hub": receives and weighs parcels and moves them along their journey.</summary>
    public const string ShipmentManager = "ShipmentManager";

    /// <summary>Job role "Support": Contact messages, later orders, team emails, chats and cases.</summary>
    public const string SupportAgent = "SupportAgent";

    /// <summary>Job role "Country manager": runs one country and approves its sensitive changes.</summary>
    public const string CountryManager = "CountryManager";

    /// <summary>Job role "Finance": payments, refunds, seller withdrawals, commission.</summary>
    public const string Finance = "Finance";

    /// <summary>Job role "Content": SEO, AI prompts, Help text and Terms drafts.</summary>
    public const string Content = "Content";

    /// <summary>Job role "Technical": settings and system health, no customer data.</summary>
    public const string Technical = "Technical";

    /// <summary>
    /// Delivers food orders and parcels in their own country: sees "My deliveries" and marks each one delivered with
    /// a photo. Sellers and parcel staff pick a courier when an order or parcel goes out for delivery.
    /// </summary>
    public const string Courier = "Courier";

    /// <summary>Phloem's staff job roles (not Admin).</summary>
    public static readonly IReadOnlyList<string> JobRoles =
    [
        CountryManager, SupportAgent, CatalogManager, ShipmentManager, Finance, Content, Technical
    ];

    /// <summary>Admin plus every job role: the people who must use two-factor sign-in.</summary>
    public static readonly IReadOnlyList<string> Staff = [Admin, .. JobRoles];

    /// <summary>
    /// The roles an admin can give or take on Admin → Users. Customer (everyone has it) and Seller (it follows the
    /// person's shop: given on approval, removed when the shop is blocked or closed) aren't in it.
    /// </summary>
    public static readonly IReadOnlyList<string> Assignable = [.. Staff, Courier];

    /// <summary>All roles, for seeding.</summary>
    public static readonly IReadOnlyList<string> All = [Admin, Customer, Seller, .. JobRoles, Courier];

    /// <summary>True for a staff job role (not Admin).</summary>
    /// <param name="role">The stored role name.</param>
    /// <returns>True when the role is in <see cref="JobRoles"/>.</returns>
    public static bool IsJobRole(string? role) =>
        role is not null && JobRoles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>The role as people read it, e.g. "SupportAgent" → "Support".</summary>
    /// <param name="role">The stored role name.</param>
    /// <returns>The display name; the stored name when the role is unknown.</returns>
    public static string DisplayName(string role) => role switch
    {
        Admin => "Admin",
        Customer => "Customer",
        Seller => "Seller",
        CountryManager => "Country manager",
        SupportAgent => "Support",
        CatalogManager => "Shops & catalogue",
        ShipmentManager => "Parcel hub",
        Finance => "Finance",
        Content => "Content",
        Technical => "Technical",
        Courier => "Courier",
        _ => role
    };

    /// <summary>What the role is for, shown on the admin screens (and stored with the role when it's first
    /// created).</summary>
    /// <param name="role">The stored role name.</param>
    /// <returns>The description; null when the role is unknown.</returns>
    public static string? Description(string role) => role switch
    {
        Admin => "Owners: everything, in every country",
        Customer => "Orders food and sends parcels (every signed-up user)",
        Seller => "Runs an approved shop: dishes, prices and orders",
        CountryManager => "Runs one country: shops, dish list, pricing, orders and parcels",
        SupportAgent => "Support: Contact messages, orders, team emails, chats and problem reports",
        CatalogManager => "Shops & catalogue: approves shops and their documents, edits the dish list",
        ShipmentManager => "Parcel hub: receives and weighs parcels and moves them along their journey",
        Finance => "Payments, refunds, seller withdrawals and commission",
        Content => "Search engine text and AI prompts",
        Technical => "Settings and system health; no customer data",
        Courier => "Delivers food orders and parcels, with a photo as proof of delivery",
        _ => null
    };
}
