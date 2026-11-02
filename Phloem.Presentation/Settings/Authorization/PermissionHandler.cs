using Microsoft.AspNetCore.Authorization;
using Phloem.Infrastructure.Settings.Identity;

namespace Phloem.Presentation.Settings.Authorization;

/// <summary>
/// Checks a <see cref="PermissionRequirement"/> against the "perm" and "cty" claims of the caller's token.
/// </summary>
/// <remarks>
/// <para><b>Permission:</b> the caller holds it, or holds "*" (Admin).</para>
/// <para><b>Countries:</b> <see cref="CountryRule.AllCountries"/> needs "*"; <see cref="CountryRule.RecordCountry"/>
/// needs at least one country (the handler then checks each record); <see cref="CountryRule.NotCountrySpecific"/>
/// checks nothing.</para>
/// <para>A failure gives 403 (Forbidden) through the JWT events' ProblemDetails.</para>
/// </remarks>
internal sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   PermissionRequirement requirement)
    {
        var user = context.User;

        // 1. Permission
        var hasPermission = user.HasClaim(JwtClaimNames.Permission, JwtClaimNames.All)
                            || user.HasClaim(JwtClaimNames.Permission, requirement.Permission);
        if (!hasPermission) return Task.CompletedTask;

        // 2. Countries
        var countriesOk = requirement.Rule switch
        {
            CountryRule.AllCountries => user.HasClaim(JwtClaimNames.Country, JwtClaimNames.All),
            CountryRule.RecordCountry => user.HasClaim(c => c.Type == JwtClaimNames.Country),
            CountryRule.NotCountrySpecific => true,
            _ => false
        };

        if (countriesOk) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
