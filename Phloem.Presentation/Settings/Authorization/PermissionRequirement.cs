using Microsoft.AspNetCore.Authorization;
using Phloem.Domain.Constants;

namespace Phloem.Presentation.Settings.Authorization;

/// <summary>"The caller must hold this permission, and cover countries as this rule says."</summary>
/// <param name="Permission">A value from <see cref="Permissions"/>.</param>
/// <param name="Rule">How countries are checked.</param>
public sealed record PermissionRequirement(string Permission, CountryRule Rule) : IAuthorizationRequirement
{
    /// <summary>Start of every permission policy name.</summary>
    public const string PolicyPrefix = "perm:";

    /// <summary>The policy name for a permission and rule, e.g. "perm:parcels.manage:RecordCountry".</summary>
    /// <param name="permission">The permission.</param>
    /// <param name="rule">The country rule.</param>
    /// <returns>The policy name <see cref="PermissionPolicyProvider"/> understands.</returns>
    public static string PolicyName(string permission, CountryRule rule) => $"{PolicyPrefix}{permission}:{rule}";

    /// <summary>Reads a policy name made by <see cref="PolicyName"/>.</summary>
    /// <param name="policyName">The policy name.</param>
    /// <param name="requirement">The requirement, when the name is a valid permission policy.</param>
    /// <returns>True when the name is a permission policy for a known permission.</returns>
    public static bool TryParse(string policyName, out PermissionRequirement? requirement)
    {
        requirement = null;
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.Ordinal)) return false;

        var body = policyName[PolicyPrefix.Length..];
        var split = body.LastIndexOf(':');
        if (split <= 0) return false;

        var permission = body[..split];
        if (!Permissions.IsKnown(permission)) return false;
        if (!Enum.TryParse<CountryRule>(body[(split + 1)..], out var rule) || !Enum.IsDefined(rule)) return false;

        requirement = new PermissionRequirement(permission, rule);
        return true;
    }
}
