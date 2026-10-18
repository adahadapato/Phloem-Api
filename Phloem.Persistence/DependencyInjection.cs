using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Persistence.Interceptors;
using Phloem.Persistence.Repositories;
using Phloem.Persistence.Seeding;

namespace Phloem.Persistence;

/// <summary>
/// Registers everything Persistence provides, in this order:
/// <list type="number">
///   <item><description><b>Connection string:</b> read and checked (fails fast if missing).</description></item>
///   <item><description><b>Interceptors:</b> the audit interceptor (CreatedAt / UpdatedAt).</description></item>
///   <item><description><b>Database context:</b> SQL Server, migrations, split queries, warnings.</description></item>
///   <item><description><b>Unit of work:</b> the same context instance, seen as <see cref="IUnitOfWork"/>.</description></item>
///   <item><description><b>Repositories:</b> one per aggregate root.</description></item>
/// </list>
/// </summary>
/// <remarks>Called once from Presentation's DependencyInjection.AddApi.</remarks>
public static class DependencyInjection
{
    /// <summary>Adds the database and its services.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration (the connection string comes from User Secrets /
    /// environment variables, never appsettings.json).</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="InvalidOperationException">No connection string configured.</exception>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);   // 1
                                                                    
        return services
            .AddInterceptors()                                       // 2
            .AddDatabase(connectionString)                           // 3
            .AddUnitOfWork()                                         // 4
            .AddRepositories()                                       // 5
            .AddSeeding(configuration);                              // 6
    }

    // ───────────────────────────── 1. Connection string ─────────────────────────────

    /// <summary>Reads the connection string and stops start-up if it's missing.</summary>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The connection string.</returns>
    /// <exception cref="InvalidOperationException">No connection string configured.</exception>
    private static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(AppSettings.ConnectionStrings.DefaultName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"'{AppSettings.ConnectionStrings.Default}' is empty. Set it in User Secrets (development) or as the " +
                "environment variable ConnectionStrings__DefaultConnection (production).");

        return connectionString;
    }

    // ───────────────────────────── 2. Interceptors ─────────────────────────────

    /// <summary>Registers the database interceptors.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <remarks>Singleton: it holds no state, only the <see cref="TimeProvider"/>.</remarks>
    private static IServiceCollection AddInterceptors(this IServiceCollection services)
    {
        services.AddSingleton<AuditInterceptor>();
        return services;
    }

    // ───────────────────────────── 3. Database context ─────────────────────────────

    /// <summary>Registers <see cref="PhloemDbContext"/> on SQL Server.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The checked connection string.</param>
    /// <returns>The same service collection, for chaining.</returns>
    private static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PhloemDbContext>((provider, options) =>
        {
            // 3a. SQL Server
            options.UseSqlServer(connectionString, sql =>
            {
                // Migrations live in this project, not in Presentation
                sql.MigrationsAssembly(typeof(PhloemDbContext).Assembly.FullName);

                // One SQL query per included collection, so a dish with 5 photos and 4 price ranges
                // doesn't come back as 20 duplicated rows (matters most for paged lists)
                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // 3b. Interceptors
            options.AddInterceptors(provider.GetRequiredService<AuditInterceptor>());

            // 3c. Warnings: expected by design, soft-deleted parents are never Include-d
            //     (see EntityConfiguration's remarks)
            options.ConfigureWarnings(warnings => warnings.Ignore(
                CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        });

        return services;
    }

    // ───────────────────────────── 4. Unit of work ─────────────────────────────

    /// <summary>Exposes the context as <see cref="IUnitOfWork"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <remarks>The context IS the unit of work: the same scoped instance the repositories use, so one
    /// <c>SaveChangesAsync</c> saves everything they changed.</remarks>
    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PhloemDbContext>());
        return services;
    }

    // ───────────────────────────── 5. Repositories ─────────────────────────────

    /// <summary>Registers one repository per aggregate root, all sharing the request's context.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // 5a. Users and shops
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();

        // 5b. Dishes
        services.AddScoped<IDishCatalogRepository, DishCatalogRepository>();
        services.AddScoped<IDishRepository, DishRepository>();

        // 5c. Pricing
        services.AddScoped<IDeliveryPricingRepository, DeliveryPricingRepository>();
        services.AddScoped<ICommissionRepository, CommissionRepository>();

        // 5d. Orders and shipping
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IShipmentRouteRepository, ShipmentRouteRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        // 5e. Sign-in (not a Domain aggregate; used by Infrastructure's Identity services)
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

   

    // ───────────────────────────── 6. Seeding ─────────────────────────────

    /// <summary>Reads the <c>SeedData</c> section for <see cref="DbSeeder"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <remarks>An unknown country name leaves <see cref="SeedDataSettings.AdminCountry"/> null, so admin seeding is
    /// skipped with a warning rather than crashing start-up.</remarks>
    private static IServiceCollection AddSeeding(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeedDataSettings>(settings =>
        {
            settings.AdminEmail = configuration[AppSettings.SeedData.AdminEmail] ?? string.Empty;
            settings.AdminPassword = configuration[AppSettings.SeedData.AdminPassword] ?? string.Empty;
            settings.AdminFirstName = configuration[AppSettings.SeedData.AdminFirstName] ?? string.Empty;
            settings.AdminLastName = configuration[AppSettings.SeedData.AdminLastName] ?? string.Empty;
            settings.AdminCountry = Enum.TryParse<Country>(configuration[AppSettings.SeedData.AdminCountry],
                                                           ignoreCase: true, out var country)
                ? country
                : null;
        });

        return services;
    }
}