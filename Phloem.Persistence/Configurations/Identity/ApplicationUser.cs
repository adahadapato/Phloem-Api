using Microsoft.AspNetCore.Identity;

namespace Phloem.Persistence.Configurations.Identity;

/// <summary>
/// The LOGIN account (table AspNetUsers): email, password hash, phone, 2FA, lockout, plus whether an admin has
/// deactivated it and when it last signed in. Managed by ASP.NET Core Identity's <c>UserManager</c>.
/// </summary>
/// <remarks>
/// <para>Deliberately thin: the person's business data (first and last name, photo, country, addresses,
/// notification switches) lives in the Domain's <c>UserProfile</c>, which has the SAME Id. Sign-up creates both in one
/// transaction. Names and photos are NOT copied here, so there's only ever one version of them
/// (<c>UserProfile.GetFullName()</c>).</para>
/// <para>Ids are version-7 Guids (time-ordered), like every Domain Id.</para>
/// <para>No configuration class: <c>IdentityDbContext</c> maps Identity's tables itself, and the two extra columns
/// map by convention.</para>
/// </remarks>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Creates a user with a new time-ordered Id.</summary>
    public ApplicationUser() => Id = Guid.CreateVersion7();

    /// <summary>
    /// False when an admin has deactivated the account: sign-in and token refresh are refused, and existing refresh
    /// tokens are revoked. Different from Identity's lockout, which is temporary and automatic (too many wrong
    /// passwords).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>When the user last signed in successfully (UTC); null if never.</summary>
    public DateTime? LastLoginAt { get; set; }
}