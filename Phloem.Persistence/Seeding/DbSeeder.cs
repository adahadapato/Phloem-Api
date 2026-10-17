using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phloem.Persistence.Configurations.Identity;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// Brings the database up to date at start-up, in this order:
/// <list type="number">
///   <item><description><b>Migrations:</b> applies any that haven't run yet.</description></item>
///   <item><description><b>Roles:</b> every role in <c>Roles.All</c>.</description></item>
///   <item><description><b>First admin:</b> from the <c>SeedData</c> settings.</description></item>
///   <item><description><b>Starter data:</b> delivery pricing, dish list, routes (next step).</description></item>
/// </list>
/// </summary>
/// <remarks>Every step is safe to repeat: it only adds what's missing. Called once from Program.cs.</remarks>
public static class DbSeeder
{
    /// <summary>Migrates and seeds the database.</summary>
    /// <param name="services">The app's root service provider (<c>app.Services</c>).</param>
    /// <param name="cancellationToken">Stops start-up.</param>
    public static async Task SeedDatabaseAsync(this IServiceProvider services,
                                               CancellationToken cancellationToken = default)
    {
        // The context and Identity managers are scoped, so they need their own scope at start-up
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbSeeder));
        var context = provider.GetRequiredService<PhloemDbContext>();

        // 1. Migrations
        await context.Database.MigrateAsync(cancellationToken);

        // 2. Roles
        await ApplicationRoleSeeder.SeedAsync(provider.GetRequiredService<RoleManager<ApplicationRole>>(), logger, cancellationToken);

        // 3. First admin
        await ApplicationUserSeeder.SeedAsync(
            provider.GetRequiredService<UserManager<ApplicationUser>>(),
            context,
            provider.GetRequiredService<IOptions<SeedDataSettings>>().Value,
            logger,
            cancellationToken);

        // 4. Starter data: next step
    }
}