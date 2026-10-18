using Phloem.Domain.Enums;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// The <c>SeedData</c> section: everything the seeder needs to create the first Admin.
/// Keys: <c>Phloem.Domain.Constants.AppSettings.SeedData</c>.
/// </summary>
public sealed class SeedDataSettings
{
    /// <summary>The first Admin's email (also their sign-in name). Empty = skip admin seeding.</summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>The first Admin's password. Empty = skip admin seeding.</summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>First name on the Admin's profile.</summary>
    public string AdminFirstName { get; set; } = string.Empty;

    /// <summary>Last name on the Admin's profile.</summary>
    public string AdminLastName { get; set; } = string.Empty;

    /// <summary>Country on the Admin's profile; null if the setting is missing or not a valid country name.</summary>
    public Country? AdminCountry { get; set; }

    /// <summary>Seed the starter data on an empty database.</summary>
    public bool StarterDataEnabled { get; set; }

    /// <summary>The starter-data JSON file, relative to the app folder (or a full path).</summary>
    public string StarterDataPath { get; set; } = string.Empty;

    /// <summary>True when every value needed to create the Admin is present.</summary>
    public bool CanSeedAdmin =>
        !string.IsNullOrWhiteSpace(AdminEmail) &&
        !string.IsNullOrWhiteSpace(AdminPassword) &&
        !string.IsNullOrWhiteSpace(AdminFirstName) &&
        !string.IsNullOrWhiteSpace(AdminLastName) &&
        AdminCountry is not null;
}