using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Phloem.Persistence;
using Phloem.Persistence.Configurations.Identity;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// Registers ASP.NET Core Identity. Called from <see cref="DependencyInjection.AddInfrastructure"/>.
/// <list type="number">
///   <item><description><b>Settings:</b> binds and checks <see cref="IdentitySettings"/> at start-up.</description></item>
///   <item><description><b>Identity:</b> UserManager, RoleManager, stores on <see cref="PhloemDbContext"/>, token
///   providers (email confirmation, password reset, 2FA).</description></item>
///   <item><description><b>Rules:</b> copies the settings into Identity's options.</description></item>
///   <item><description><b>Data protection:</b> signs the email / reset tokens.</description></item>
///   <item><description><b>JWT settings:</b> binds and checks <see cref="JwtSettings"/> at start-up.</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para><b>AddIdentityCore, not AddIdentity:</b> no cookies. The API signs in with JWT, so only the managers and
/// stores are needed.</para>
/// <para><b>Same database context</b> as the repositories (scoped), so sign-up can save the account and its
/// profile in one transaction.</para>
/// </remarks>
internal static class IdentitySetup
{
    /// <summary>Adds Identity with Phloem's rules.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration (appsettings, secrets, environment variables).</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPhloemIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Settings
        services.AddOptions<IdentitySettings>()
                .Bind(configuration.GetSection(IdentitySettings.SectionName))
                .ValidateDataAnnotations()
                .Validate(settings => IsValid(settings.Password) && IsValid(settings.Lockout),
                          "Identity:Password or Identity:Lockout has a value out of range.")
                .ValidateOnStart();

        // 2. Identity
        services.AddIdentityCore<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<PhloemDbContext>()
                .AddDefaultTokenProviders();

        // 3. Rules, from the settings (read when Identity first needs them)
        services.AddOptions<IdentityOptions>()
                .Configure<IOptions<IdentitySettings>>((options, settings) => Apply(options, settings.Value));

        // 4. Data protection
        services.AddDataProtection();

        // 5. JWT settings (the token service and JWT authentication come with sign-in)
        services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        return services;
    }

    /// <summary>Copies Phloem's settings into Identity's options.</summary>
    /// <param name="options">Identity's options.</param>
    /// <param name="settings">The <c>Identity</c> section.</param>
    private static void Apply(IdentityOptions options, IdentitySettings settings)
    {
        // Users: sign-in is by email, so it must be unique (a rule, not a setting)
        options.User.RequireUniqueEmail = true;

        // Sign-in
        options.SignIn.RequireConfirmedEmail = settings.RequireConfirmedEmail;

        // Password
        options.Password.RequiredLength = settings.Password.RequiredLength;
        options.Password.RequireDigit = settings.Password.RequireDigit;
        options.Password.RequireLowercase = settings.Password.RequireLowercase;
        options.Password.RequireUppercase = settings.Password.RequireUppercase;
        options.Password.RequireNonAlphanumeric = settings.Password.RequireNonAlphanumeric;
        options.Password.RequiredUniqueChars = settings.Password.RequiredUniqueChars;

        // Lockout
        options.Lockout.MaxFailedAccessAttempts = settings.Lockout.MaxFailedAccessAttempts;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(settings.Lockout.LockoutMinutes);
        options.Lockout.AllowedForNewUsers = settings.Lockout.AllowedForNewUsers;
    }

    /// <summary>Runs the data-annotation checks on a nested settings object.</summary>
    /// <param name="settings">The nested settings.</param>
    /// <returns>True when present and every value is in range.</returns>
    private static bool IsValid(object? settings) =>
        settings is not null &&
        Validator.TryValidateObject(settings, new ValidationContext(settings), null, validateAllProperties: true);
}