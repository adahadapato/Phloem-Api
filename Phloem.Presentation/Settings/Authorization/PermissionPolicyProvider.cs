using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Phloem.Presentation.Settings.Authorization;

/// <summary>
/// Builds a policy for any "perm:…" name on demand, so there's no need to register one policy per permission.
/// Every other name goes to ASP.NET's default provider.
/// </summary>
/// <param name="options">Authorization options (for the default provider).</param>
internal sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    /// <inheritdoc />
    /// <remarks>A "perm:…" name with an unknown permission returns null, which ASP.NET treats as an error at the
    /// first request, so a typo can't silently open an endpoint.</remarks>
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PermissionRequirement.PolicyPrefix, StringComparison.Ordinal))
            return await base.GetPolicyAsync(policyName);

        if (!PermissionRequirement.TryParse(policyName, out var requirement)) return null;

        return new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .AddRequirements(requirement!)
            .Build();
    }
}
