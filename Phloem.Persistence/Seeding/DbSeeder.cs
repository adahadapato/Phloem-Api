
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phloem.Persistence.Configurations.Identity;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// Applies database migrations and seeds required application data.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Migrates and seeds the database during application startup.
    /// </summary>
    public static async Task SeedDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var provider = scope.ServiceProvider;

        var logger = provider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DbSeeder));

        try
        {
            logger.LogInformation(
                "Database initialization: resolving DbContext.");

            var context =
                provider.GetRequiredService<PhloemDbContext>();

            // 1. Database migrations
            logger.LogInformation(
                "Database initialization STEP 1: starting migrations.");

            try
            {
                await context.Database.MigrateAsync(cancellationToken);

                logger.LogInformation(
                    "Database initialization STEP 1: migrations completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Database initialization STEP 1 FAILED: migrations.");

                throw;
            }

            // 2. Roles
            logger.LogInformation(
                "Database initialization STEP 2: starting role seeding.");

            try
            {
                await ApplicationRoleSeeder.SeedAsync(
                    provider.GetRequiredService<RoleManager<ApplicationRole>>(),
                    logger,
                    cancellationToken);

                logger.LogInformation(
                    "Database initialization STEP 2: role seeding completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Database initialization STEP 2 FAILED: role seeding.");

                throw;
            }

            // 3. First administrator
            logger.LogInformation(
                "Database initialization STEP 3: starting admin seeding.");

            try
            {
                await ApplicationUserSeeder.SeedAsync(
                    provider.GetRequiredService<UserManager<ApplicationUser>>(),
                    context,
                    provider.GetRequiredService<IOptions<SeedDataSettings>>().Value,
                    logger,
                    cancellationToken);

                logger.LogInformation(
                    "Database initialization STEP 3: admin seeding completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Database initialization STEP 3 FAILED: admin seeding.");

                throw;
            }

            // 4. Starter data
            logger.LogInformation(
                "Database initialization STEP 4: starting starter data seeding.");

            try
            {
                await StarterDataSeeder.SeedAsync(
                    context,
                    provider.GetRequiredService<IOptions<SeedDataSettings>>().Value,
                    logger,
                    cancellationToken);

                logger.LogInformation(
                    "Database initialization STEP 4: starter data completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Database initialization STEP 4 FAILED: starter data.");

                throw;
            }

            logger.LogInformation(
                "All database initialization steps completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "Database initialization failed. Application startup cannot continue.");

            throw;
        }
    }
}
