using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;

namespace Phloem.Persistence.Configurations.Identity;

/// <summary>
/// Creates every role in <see cref="Roles.All"/> that doesn't exist yet, with its description.
/// </summary>
/// <remarks>
/// <para>Safe to run on every start-up: existing roles are left alone, except that a missing description is filled in.</para>
/// <para>A role in <see cref="Roles.All"/> without a description here still gets created (description null).</para>
/// </remarks>
internal static class ApplicationRoleSeeder
{
    /// <summary>What each role can do, shown on the admin screens.</summary>
    private static readonly IReadOnlyDictionary<string, string> Descriptions = new Dictionary<string, string>
    {
        [Roles.Admin] = "Everything: approves sellers, blocks shops, manages users and staff",
        [Roles.Customer] = "Orders food and sends parcels (every signed-up user)",
        [Roles.Seller] = "Runs an approved shop: dishes, prices and orders",
        [Roles.CatalogManager] = "Reviews dish-name suggestions and edits the dish list",
        [Roles.ShipmentManager] = "Receives parcels at the hub and moves them along their journey",
        [Roles.SupportAgent] = "Looks up orders and users, and cancels orders for customers",
        [Roles.Courier] = "Delivers food orders and parcels, with a photo as proof of delivery"
    };

    /// <summary>Creates the missing roles.</summary>
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

            var description = Descriptions.GetValueOrDefault(roleName);
            var role = await roleManager.FindByNameAsync(roleName);

            // 1. New role
            if (role is null)
            {
                IdentityResultCheck.Ensure(await roleManager.CreateAsync(new ApplicationRole(roleName, description)),
                                           $"create role '{roleName}'");
                logger.LogInformation("Seeded role {Role}", roleName);
                continue;
            }

            // 2. Existing role without a description
            if (role.Description is null && description is not null)
            {
                role.Description = description;
                IdentityResultCheck.Ensure(await roleManager.UpdateAsync(role), $"update role '{roleName}'");
            }
        }
    }
}