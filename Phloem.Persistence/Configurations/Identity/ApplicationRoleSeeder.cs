using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;

namespace Phloem.Persistence.Configurations.Identity;

/// <summary>
/// Creates every role in <see cref="Roles.All"/> that doesn't exist yet, with its description, and gives each staff
/// job role its starting permissions.
/// </summary>
/// <remarks>
/// <para>Safe to run on every start-up: existing roles are left alone, except that a missing description is filled in.</para>
/// <para>A role in <see cref="Roles.All"/> without a description here still gets created (description null).</para>
/// <para><b>Permissions:</b> a job role with NO permission claims yet gets its <see cref="JobRolePresets"/> set. A role
/// that already has permissions keeps them, so the owners' later changes are never overwritten. Unknown permissions
/// (e.g. one removed from the code) are left in place but do nothing.</para>
/// </remarks>
internal static class ApplicationRoleSeeder
{
    /// <summary>Creates the missing roles and gives new job roles their starting permissions.</summary>
    /// <param name="roleManager">Identity's role manager.</param>
    /// <param name="logger">Start-up log.</param>
    /// <param name="cancellationToken">Stops seeding.</param>
    /// <exception cref="InvalidOperationException">Identity refused to create or update a role.</exception>
    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager, ILogger logger,
                                       CancellationToken cancellationToken = default)
    {
        foreach (var roleName in Roles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var description = Roles.Description(roleName);
            var role = await roleManager.FindByNameAsync(roleName);

            // 1. New role
            if (role is null)
            {
                role = new ApplicationRole(roleName, description);
                IdentityResultCheck.Ensure(await roleManager.CreateAsync(role), $"create role '{roleName}'");
                logger.LogInformation("Seeded role {Role}", roleName);
            }

            // 2. Existing role without a description
            else if (role.Description is null && description is not null)
            {
                role.Description = description;
                IdentityResultCheck.Ensure(await roleManager.UpdateAsync(role), $"update role '{roleName}'");
            }

            // 3. Starting permissions (job roles only, and only while they have none)
            await SeedPermissionsAsync(roleManager, role, logger);
        }
    }

    /// <summary>Adds the preset permissions to a job role that has no permission claims yet.</summary>
    /// <param name="roleManager">Identity's role manager.</param>
    /// <param name="role">The role (already saved).</param>
    /// <param name="logger">Start-up log.</param>
    /// <exception cref="InvalidOperationException">Identity refused to add a claim.</exception>
    private static async Task SeedPermissionsAsync(RoleManager<ApplicationRole> roleManager, ApplicationRole role,
                                                   ILogger logger)
    {
        if (role.Name is null || !JobRolePresets.Permissions.TryGetValue(role.Name, out var preset)) return;

        var claims = await roleManager.GetClaimsAsync(role);
        if (claims.Any(c => c.Type == ApplicationRole.PermissionClaimType)) return;

        foreach (var permission in preset)
            IdentityResultCheck.Ensure(
                await roleManager.AddClaimAsync(role, new Claim(ApplicationRole.PermissionClaimType, permission)),
                $"give role '{role.Name}' permission '{permission}'");

        logger.LogInformation("Seeded {Count} permissions for role {Role}", preset.Count, role.Name);
    }
}
