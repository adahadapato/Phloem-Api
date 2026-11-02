namespace Phloem.Domain.Constants;

/// <summary>
/// The starting permissions of each staff job role.
/// </summary>
/// <remarks>
/// <para><b>Seeded once:</b> at start-up, a job role with NO permissions yet gets its preset (as role claims). A role
/// that already has permissions is left alone, so changes the owners make later on the Staff page are never
/// overwritten.</para>
/// <para><b>Admin isn't here:</b> it always holds <see cref="Permissions.Everything"/>, worked out in code.</para>
/// <para><b>Countries come separately:</b> a Ghana "Parcel hub" person and a Nigeria one hold the same permissions but
/// see different parcels (<c>StaffCountry</c>).</para>
/// </remarks>
public static class JobRolePresets
{
    /// <summary>Starting permissions per job role (keys are the stored role names in <see cref="Roles"/>).</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Permissions =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // Runs one country; approver for that country's sensitive changes
            [Roles.CountryManager] =
            [
                Constants.Permissions.Shops.Manage,
                Constants.Permissions.Catalog.Manage,
                Constants.Permissions.Pricing.Manage,
                Constants.Permissions.Orders.View,
                Constants.Permissions.Parcels.Manage
            ],

            // "Support"
            [Roles.SupportAgent] =
            [
                Constants.Permissions.Messages.Contact,
                Constants.Permissions.Orders.View,
                Constants.Permissions.Messages.ReadEmails,
                Constants.Permissions.Messages.ReplyEmails,
                Constants.Permissions.Messages.ReadChats,
                Constants.Permissions.Messages.Cases
            ],

            // "Shops & catalogue"
            [Roles.CatalogManager] =
            [
                Constants.Permissions.Shops.Manage,
                Constants.Permissions.Catalog.Manage
            ],

            // "Parcel hub"
            [Roles.ShipmentManager] =
            [
                Constants.Permissions.Parcels.Manage
            ],

            [Roles.Finance] =
            [
                Constants.Permissions.Finance.Commission,
                Constants.Permissions.Finance.ViewPayments,
                Constants.Permissions.Finance.IssueRefunds,
                Constants.Permissions.Finance.ApproveWithdrawals,
                Constants.Permissions.Orders.View
            ],

            [Roles.Content] =
            [
                Constants.Permissions.Content.Seo,
                Constants.Permissions.Content.AiPrompts
            ],

            [Roles.Technical] =
            [
                Constants.Permissions.Platform.Settings
            ]
        };
}
