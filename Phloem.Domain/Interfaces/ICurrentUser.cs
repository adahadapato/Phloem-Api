using Phloem.Domain.Enums;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Who is making the current request.
/// Handlers use this instead of taking a userId from the request body,
/// so nobody can act as someone else by sending a different Id.
///
/// Implemented in Presentation (reads the JWT claims from HttpContext).
/// </summary>
/// <remarks>
/// <para><b>Permissions and countries</b> come from the sign-in token, so a change made on the Staff page takes
/// effect at the person's next token refresh (a few minutes), or at once when their sessions are ended.</para>
/// <para>Admins hold every permission and cover every country.</para>
/// </remarks>
public interface ICurrentUser
{
    /// <summary>The signed-in user's Id (same as the UserProfile Id); null if not signed in.</summary>
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    /// <summary>Check a role, e.g. IsInRole(Roles.Admin).</summary>
    bool IsInRole(string role);

    /// <summary>True when the person holds this permission (always true for an Admin).</summary>
    /// <param name="permission">A value from <see cref="Constants.Permissions"/>.</param>
    bool HasPermission(string permission);

    /// <summary>True when the person covers every country (Admins, and staff given "all countries").</summary>
    bool CoversAllCountries { get; }

    /// <summary>The countries the person covers; empty for non-staff and when <see cref="CoversAllCountries"/>.</summary>
    IReadOnlyCollection<Country> Countries { get; }

    /// <summary>True when the person covers this country (always true with <see cref="CoversAllCountries"/>).</summary>
    /// <param name="country">The record's country.</param>
    bool CoversCountry(Country country);
}
