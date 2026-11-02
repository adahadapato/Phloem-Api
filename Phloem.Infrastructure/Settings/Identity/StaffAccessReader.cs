using Microsoft.AspNetCore.Identity;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Persistence.Configurations.Identity;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// Works out what goes into a person's sign-in token as staff permissions and countries.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><description><b>Admin:</b> permission "*" and country "*".</description></item>
///   <item><description><b>Job roles:</b> every permission claim of every job role they hold (role claims of type
///   <see cref="ApplicationRole.PermissionClaimType"/>; unknown permissions are dropped), and the countries in their
///   <c>StaffCountry</c> rows ("*" for the all-countries row).</description></item>
///   <item><description><b>Everyone else:</b> nothing.</description></item>
/// </list>
/// <para>Read at every sign-in and every token refresh, so a change on the Staff page reaches the person within one
/// access-token lifetime.</para>
/// </remarks>
internal sealed class StaffAccessReader(RoleManager<ApplicationRole> roleManager,
                                        IStaffCountryRepository staffCountries)
{
    /// <summary>Reads a person's staff permissions and countries.</summary>
    /// <param name="userId">The person signing in.</param>
    /// <param name="roles">Their roles (already read).</param>
    /// <param name="ct">Stops the reads.</param>
    /// <returns>What to put in the token; <see cref="StaffAccess.None"/> when they aren't staff.</returns>
    public async Task<StaffAccess> ReadAsync(Guid userId, IReadOnlyCollection<string> roles,
                                             CancellationToken ct = default)
    {
        // 1. Admin: everything, everywhere
        if (roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase))
            return new StaffAccess([JwtClaimNames.All], [JwtClaimNames.All]);

        // 2. Not staff
        var jobRoles = roles.Where(Roles.IsJobRole).ToList();
        if (jobRoles.Count == 0) return StaffAccess.None;

        // 3. Permissions from the job roles
        var permissions = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var roleName in jobRoles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            foreach (var claim in await roleManager.GetClaimsAsync(role))
                if (claim.Type == ApplicationRole.PermissionClaimType && Permissions.IsKnown(claim.Value))
                    permissions.Add(claim.Value);
        }

        // 4. Countries
        var rows = await staffCountries.GetForUserAsync(userId, ct);
        IReadOnlyList<string> countries = rows.Any(r => r.IsAllCountries)
            ? [JwtClaimNames.All]
            : rows.Select(r => r.Country!.Value.ToString()).Distinct().Order().ToList();

        return new StaffAccess(permissions.ToList(), countries);
    }
}

/// <summary>Staff permissions and countries for a sign-in token.</summary>
/// <param name="Permissions">Permission values, or a single "*" for Admin.</param>
/// <param name="Countries">Country names, or a single "*" for every country.</param>
internal sealed record StaffAccess(IReadOnlyList<string> Permissions, IReadOnlyList<string> Countries)
{
    /// <summary>Not staff: no permissions, no countries.</summary>
    public static StaffAccess None { get; } = new([], []);
}
