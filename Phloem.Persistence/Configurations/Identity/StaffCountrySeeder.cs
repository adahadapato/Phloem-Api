using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations.Identity;

/// <summary>
/// Gives every staff member who covers no country yet their own home country (from their profile).
/// </summary>
/// <remarks>
/// <para><b>Why:</b> before staff access existed, a Parcel hub (ShipmentManager) or Support person could see every
/// country. When this release starts, each of them is limited to their profile's country; the owners can widen it on
/// the Staff page.</para>
/// <para>Safe to run on every start-up: people who already cover a country are left alone. Admins are skipped (they
/// cover every country in code).</para>
/// </remarks>
internal static class StaffCountrySeeder
{
    /// <summary>Adds the home-country row for staff without any.</summary>
    /// <param name="userManager">Identity's user manager (to find who has a job role).</param>
    /// <param name="context">The database.</param>
    /// <param name="logger">Start-up log.</param>
    /// <param name="cancellationToken">Stops seeding.</param>
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, PhloemDbContext context,
                                       ILogger logger, CancellationToken cancellationToken = default)
    {
        // 1. Everyone with a job role
        var staffIds = new HashSet<Guid>();
        foreach (var role in Roles.JobRoles)
            foreach (var user in await userManager.GetUsersInRoleAsync(role))
                staffIds.Add(user.Id);

        if (staffIds.Count == 0) return;
        var ids = staffIds.ToList();   // a plain list translates to SQL everywhere

        // 2. Those who already cover a country
        var covered = await context.StaffCountries
            .Where(s => ids.Contains(s.UserId))
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var missing = ids.Except(covered).ToList();
        if (missing.Count == 0) return;

        // 3. Their home countries
        var homes = await context.UserProfiles.AsNoTracking()
            .Where(p => missing.Contains(p.Id))
            .Select(p => new { p.Id, p.Country })
            .ToListAsync(cancellationToken);

        foreach (var home in homes)
            context.StaffCountries.Add(StaffCountry.ForCountry(home.Id, home.Country));

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Gave {Count} staff members their home country", homes.Count);
    }
}
