namespace Phloem.Domain.Constants;

/// <summary>
/// Everything a staff member can be allowed to do. Endpoints ask for one of these, never for a role.
/// </summary>
/// <remarks>
/// <para><b>Fixed in code:</b> a permission only means something once an endpoint asks for it, so the list lives here.
/// Which job role holds which permissions is DATA (role claims, seeded from <see cref="JobRolePresets"/> and edited
/// later by the owners), so a role can change without code.</para>
/// <para><b>Admin</b> holds every permission (<see cref="Everything"/>). Customers, sellers and couriers hold none:
/// their endpoints check ownership instead.</para>
/// <para>Some permissions are listed before the feature that uses them exists (orders, refunds, chats, emails, SEO, AI
/// prompts…), so the job-role presets are complete from day one. Until an endpoint asks for one, holding it does
/// nothing.</para>
/// <para>Values are short and lower-case ("area.action") because every permission a person holds travels in their
/// sign-in token.</para>
/// </remarks>
public static class Permissions
{
    /// <summary>Stands for every permission. Only Admin gets it.</summary>
    public const string Everything = "*";

    /// <summary>People's accounts.</summary>
    public static class Users
    {
        /// <summary>List users, suspend and reinstate them.</summary>
        public const string Manage = "users.manage";
    }

    /// <summary>Phloem's own team.</summary>
    public static class Staff
    {
        /// <summary>Give or take job roles and countries (Admin is only ever given by an Admin).</summary>
        public const string Manage = "staff.manage";
    }

    /// <summary>Shops (vendors).</summary>
    public static class Shops
    {
        /// <summary>Approve, reject, verify and block shops; review their documents.</summary>
        public const string Manage = "shops.manage";
    }

    /// <summary>The dish list.</summary>
    public static class Catalog
    {
        /// <summary>Dish names, dish-name suggestions and price ranges.</summary>
        public const string Manage = "catalog.manage";
    }

    /// <summary>What buyers pay for delivery.</summary>
    public static class Pricing
    {
        /// <summary>Delivery fee ranges and delivery-time charges per country.</summary>
        public const string Manage = "pricing.manage";
    }

    /// <summary>Money.</summary>
    public static class Finance
    {
        /// <summary>Commission rates per country.</summary>
        public const string Commission = "commission.manage";

        /// <summary>See payments.</summary>
        public const string ViewPayments = "payments.view";

        /// <summary>Refund buyers (large refunds will need a second person).</summary>
        public const string IssueRefunds = "refunds.issue";

        /// <summary>Approve seller withdrawals.</summary>
        public const string ApproveWithdrawals = "withdrawals.approve";
    }

    /// <summary>Food orders.</summary>
    public static class Orders
    {
        /// <summary>See any order (staff order pages).</summary>
        public const string View = "orders.view";
    }

    /// <summary>Parcels between countries.</summary>
    public static class Parcels
    {
        /// <summary>Receive and weigh parcels, move them along, pick parcel couriers.</summary>
        public const string Manage = "parcels.manage";

        /// <summary>Routes, rates, hubs and parcel item rules.</summary>
        public const string Settings = "parcels.settings";
    }

    /// <summary>Messages from and to people.</summary>
    public static class Messages
    {
        /// <summary>Read and answer Contact us messages.</summary>
        public const string Contact = "contact.read";

        /// <summary>Read the team mailboxes.</summary>
        public const string ReadEmails = "emails.read";

        /// <summary>Reply from the team mailboxes.</summary>
        public const string ReplyEmails = "emails.reply";

        /// <summary>Read order chats (only for a report or dispute).</summary>
        public const string ReadChats = "chats.read";

        /// <summary>Handle "Report a problem" cases.</summary>
        public const string Cases = "cases.manage";
    }

    /// <summary>Words on the website and in the app.</summary>
    public static class Content
    {
        /// <summary>Publish Terms, Privacy and Help (will need a second person).</summary>
        public const string PublishLegal = "legal.publish";

        /// <summary>Search engine titles and descriptions.</summary>
        public const string Seo = "seo.manage";

        /// <summary>The prompts the AI features use.</summary>
        public const string AiPrompts = "ai.prompts.manage";
    }

    /// <summary>Running the system.</summary>
    public static class Platform
    {
        /// <summary>Settings and system health (no customer data).</summary>
        public const string Settings = "settings.manage";

        /// <summary>Read the staff audit log.</summary>
        public const string ViewAudit = "audit.view";
    }

    /// <summary>Every permission, in the order the Staff page lists them.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        Users.Manage, Staff.Manage,
        Shops.Manage, Catalog.Manage, Pricing.Manage,
        Finance.Commission, Finance.ViewPayments, Finance.IssueRefunds, Finance.ApproveWithdrawals,
        Orders.View,
        Parcels.Manage, Parcels.Settings,
        Messages.Contact, Messages.ReadEmails, Messages.ReplyEmails, Messages.ReadChats, Messages.Cases,
        Content.PublishLegal, Content.Seo, Content.AiPrompts,
        Platform.Settings, Platform.ViewAudit
    ];

    /// <summary>True for a permission in <see cref="All"/>.</summary>
    /// <param name="permission">The permission to check, e.g. "shops.manage".</param>
    /// <returns>True when Phloem knows this permission.</returns>
    public static bool IsKnown(string? permission) =>
        permission is not null && All.Contains(permission, StringComparer.Ordinal);
}
