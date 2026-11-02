using Microsoft.AspNetCore.Authorization;
using Phloem.Domain.Constants;

namespace Phloem.Presentation.Settings.Authorization;

/// <summary>
/// Permission-based authorization: endpoints ask for a permission (<see cref="Permissions"/>), never for a role.
/// </summary>
/// <remarks>
/// <para><b>Registering:</b> <see cref="AddPermissionAuthorization"/> (called from the authentication setup).</para>
/// <para><b>Using:</b> <c>.RequirePermission(Permissions.Shops.Manage)</c> on an endpoint or group, with a
/// <see cref="CountryRule"/> when the handler checks each record's country.</para>
/// <para><b>No blanket "admins pass" rule:</b> Admin passes because its token holds permission "*" and country
/// "*".</para>
/// </remarks>
public static class AuthorizationSetup
{
    /// <summary>Adds authorization with the permission policy provider and handler.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        return services;
    }

    /// <summary>Requires the caller to hold a staff permission.</summary>
    /// <typeparam name="TBuilder">An endpoint or group builder.</typeparam>
    /// <param name="builder">The endpoint or group.</param>
    /// <param name="permission">A value from <see cref="Permissions"/>.</param>
    /// <param name="rule">How countries are checked; the safe default needs every country.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentException">The permission isn't in <see cref="Permissions.All"/> (caught at
    /// start-up, when endpoints are mapped).</exception>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission,
                                                       CountryRule rule = CountryRule.AllCountries)
        where TBuilder : IEndpointConventionBuilder
    {
        if (!Permissions.IsKnown(permission))
            throw new ArgumentException($"Unknown permission '{permission}'. Add it to Permissions first.",
                                        nameof(permission));

        return builder.RequireAuthorization(PermissionRequirement.PolicyName(permission, rule));
    }
}
