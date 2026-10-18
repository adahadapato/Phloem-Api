using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.ValueObjects;
using Phloem.Persistence.Configurations.Identity;
using Phloem.Persistence.Converters;

namespace Phloem.Persistence;

/// <summary>
/// The one database context for Phloem: Identity's tables (users, roles, logins, tokens) AND every Domain table.
/// Also the <see cref="IUnitOfWork"/>: repositories add and change entities, and one
/// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> saves them all in one transaction.
/// </summary>
/// <remarks>
/// <para><b>One context, one scope:</b> registered per request, so Identity's UserManager and our repositories share
/// the same instance. That's what lets sign-up save the login and the profile together.</para>
/// <para><b>Mappings</b> come from the <c>IEntityTypeConfiguration</c> classes in <c>Configurations/</c>, all loaded
/// with <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>; nothing is configured inline here.</para>
/// <para><b>Audit columns</b> (CreatedAt / UpdatedAt) are filled by the audit interceptor, registered in
/// DependencyInjection.</para>
/// </remarks>
public sealed class PhloemDbContext(DbContextOptions<PhloemDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IUnitOfWork
{
    // ---------- Aggregate roots (children are reached through them) ----------

    /// <summary>Accounts' business side (with their delivery addresses).</summary>
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    /// <summary>Shops (with their documents).</summary>
    public DbSet<Vendor> Vendors => Set<Vendor>();

    /// <summary>Approved dish names (with their price ranges per country).</summary>
    public DbSet<DishCatalog> DishCatalogs => Set<DishCatalog>();

    /// <summary>Shops' dishes (with their photos).</summary>
    public DbSet<Dish> Dishes => Set<Dish>();

    /// <summary>Delivery fee ranges per country (with their window charges).</summary>
    public DbSet<DeliveryPricing> DeliveryPricings => Set<DeliveryPricing>();

    /// <summary>Commission rate per country.</summary>
    public DbSet<Commission> Commissions => Set<Commission>();

    /// <summary>Food orders (with their lines).</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>Shipping routes (with their rates).</summary>
    public DbSet<ShipmentRoute> ShipmentRoutes => Set<ShipmentRoute>();

    /// <summary>Parcels (with their items and timeline).</summary>
    public DbSet<Shipment> Shipments => Set<Shipment>();

    /// <summary>Hashed refresh tokens (sign-in), managed by the Identity services.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Identity's tables first, then every configuration in this assembly.</summary>
    /// <param name="builder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);   // Identity's tables (must come first)
        builder.ApplyConfigurationsFromAssembly(typeof(PhloemDbContext).Assembly);
    }

    /// <summary>Defaults for every column of a type, unless a configuration says otherwise.</summary>
    /// <param name="configurationBuilder">The conventions builder.</param>
    /// <remarks>
    /// <para><b>Money:</b> any decimal without an explicit precision gets decimal(18,2) instead of SQL Server's
    /// default (with a warning), so money is never silently rounded.</para>
    /// <para><b>Value objects stored as one column:</b> every <see cref="EmailAddress"/> and <see cref="PhoneNumber"/>
    /// property, on any entity, uses its converter and column size. Without this, EF would mistake an unconfigured one
    /// for a related table.</para>
    /// <para><b>Dates:</b> every DateTime is UTC and read back marked as UTC.</para>
    /// </remarks>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 1. Money
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);

        // 2. Single-column value objects
        configurationBuilder.Properties<EmailAddress>()
                            .HaveConversion<EmailAddressConverter>()
                            .HaveMaxLength(EmailAddress.MaxLength);

        configurationBuilder.Properties<PhoneNumber>()
                            .HaveConversion<PhoneNumberConverter>()
                            .HaveMaxLength(PhoneNumberConverter.MaxLength);

        // 3. Dates: stored as UTC, read back marked as UTC (so JSON writes the "Z")
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Starts a database transaction on this context. Everything saved through this context (UserManager,
    /// repositories) until <see cref="ITransaction.CommitAsync"/> is committed together, or rolled back together.
    /// </remarks>
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        new EfTransaction(await Database.BeginTransactionAsync(cancellationToken));
}