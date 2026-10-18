using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// The <c>"Jwt"</c> section of appsettings.json: how sign-in tokens are signed and how long they last.
/// Keys are listed in <see cref="AppSettings.Jwt"/>.
/// </summary>
/// <remarks>
/// <para>Checked when the app starts (see <see cref="IdentitySetup"/>): an empty or short key stops the app
/// immediately instead of failing on the first sign-in.</para>
/// <para><b>Never put the key in appsettings.json.</b> User Secrets in development
/// (<c>dotnet user-secrets set "Jwt:Key" "..."</c>), GitHub Secrets / host environment variables in production
/// (<c>Jwt__Key</c>). Changing the key signs everyone out (their access tokens stop validating).</para>
/// </remarks>
public sealed class JwtSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Jwt.Section;

    /// <summary>Shortest key HS256 accepts: 256 bits = 32 bytes.</summary>
    public const int MinimumKeyLength = 32;

    /// <summary>Signs every token. At least <see cref="MinimumKeyLength"/> characters.</summary>
    [Required(ErrorMessage = "Jwt:Key is empty. Set it in User Secrets (development) or as Jwt__Key (production).")]
    [MinLength(MinimumKeyLength, ErrorMessage = "Jwt:Key must be at least 32 characters.")]
    public string Key { get; set; } = string.Empty;

    /// <summary>Who signed the token.</summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Who the token is for.</summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>Access-token lifetime in minutes (1–1440).</summary>
    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 15;

    /// <summary>Refresh-token lifetime in days (1–365).</summary>
    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 30;

    /// <summary>Minutes allowed to type the 2FA code after the password (1–30).</summary>
    [Range(1, 30)]
    public int TwoFactorChallengeMinutes { get; set; } = 5;

    /// <summary>
    /// The key that signs (Infrastructure) and checks (Presentation) every access token. Both sides call this, so
    /// they always use the same bytes.
    /// </summary>
    /// <returns>An HMAC key built from <see cref="Key"/> (UTF-8).</returns>
    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(Key));
}