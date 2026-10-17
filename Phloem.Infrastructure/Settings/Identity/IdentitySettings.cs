using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// The <c>"Identity"</c> section of appsettings.json: sign-in rules. Keys are listed in
/// <see cref="AppSettings.Identity"/>.
/// </summary>
/// <remarks>Checked when the app starts (see <see cref="IdentitySetup"/>), so an impossible value (e.g. a password
/// length of 0) stops the app immediately.</remarks>
public sealed class IdentitySettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Identity.Section;

    /// <summary>
    /// Refuse sign-in until the user has confirmed their email. False = they can sign in straight away and
    /// confirming is only shown as a badge.
    /// </summary>
    public bool RequireConfirmedEmail { get; set; }

    /// <summary>Password strength.</summary>
    [Required]
    public PasswordSettings Password { get; set; } = new();

    /// <summary>Lockout after wrong passwords.</summary>
    [Required]
    public LockoutSettings Lockout { get; set; } = new();
}