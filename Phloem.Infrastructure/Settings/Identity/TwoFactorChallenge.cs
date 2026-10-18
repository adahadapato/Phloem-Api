using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Phloem.Persistence.Configurations.Identity;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// The short-lived "password was right, now type your 2FA code" token, handed out by login instead of real tokens.
/// </summary>
/// <remarks>
/// <para><b>Encrypted and time-limited</b> with data protection: the app can't read or change it, and it stops working
/// after <see cref="JwtSettings.TwoFactorChallengeMinutes"/>.</para>
/// <para><b>Tied to the security stamp:</b> if the password changes (or 2FA is reset) in the meantime, the stamp
/// changes and the challenge no longer works.</para>
/// </remarks>
internal sealed class TwoFactorChallenge
{
    /// <summary>Keeps these tokens separate from every other protected value (versioned, so the format can change).</summary>
    private const string Purpose = "Phloem.TwoFactorChallenge.v1";

    /// <summary>Between the user Id and the security stamp inside the token.</summary>
    private const char Separator = '|';

    private readonly ITimeLimitedDataProtector protector;
    private readonly IOptions<JwtSettings> options;

    /// <summary>Creates the challenge maker.</summary>
    /// <param name="provider">Data protection (registered in <see cref="IdentitySetup"/>).</param>
    /// <param name="options">The <c>Jwt</c> section (challenge lifetime).</param>
    public TwoFactorChallenge(IDataProtectionProvider provider, IOptions<JwtSettings> options)
    {
        protector = provider.CreateProtector(Purpose).ToTimeLimitedDataProtector();
        this.options = options;
    }

    /// <summary>Creates a challenge for a user whose password was just accepted.</summary>
    /// <param name="user">The user.</param>
    /// <returns>The challenge token for the app.</returns>
    public string Create(ApplicationUser user) =>
        protector.Protect($"{user.Id}{Separator}{user.SecurityStamp}",
                          TimeSpan.FromMinutes(options.Value.TwoFactorChallengeMinutes));

    /// <summary>Reads a challenge back.</summary>
    /// <param name="token">The challenge the app sent.</param>
    /// <param name="userId">The user it was made for.</param>
    /// <param name="securityStamp">The user's security stamp when it was made.</param>
    /// <returns>False when it's empty, tampered with or expired.</returns>
    public bool TryRead(string token, out Guid userId, out string securityStamp)
    {
        userId = Guid.Empty;
        securityStamp = string.Empty;
        if (string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            var parts = protector.Unprotect(token).Split(Separator, 2);
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out userId)) return false;

            securityStamp = parts[1];
            return true;
        }
        catch (CryptographicException)   // tampered with, made with another key, or expired
        {
            return false;
        }
    }
}