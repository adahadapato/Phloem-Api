using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Infrastructure.Settings.Identity;
using Phloem.Persistence.Configurations.Identity;

namespace Phloem.Infrastructure.Services.Identity;

/// <summary>
/// <see cref="IUserRoleService"/> with ASP.NET Core Identity: giving and taking roles from OTHER users
/// (e.g. Seller when a shop is approved).
/// </summary>
/// <remarks>
/// <para><b>Assign only:</b> roles themselves are fixed (<see cref="Roles.All"/>, seeded at start-up); unknown names
/// are refused.</para>
/// <para><b>Repeat-safe:</b> adding a role the user already has, or removing one they don't, succeeds.</para>
/// <para><b>When it takes effect:</b> roles travel inside the access token, so the user gets the change at their next
/// token refresh (at most <c>Jwt:ExpiryMinutes</c> later).</para>
/// <para><b>Job role permissions</b> are role claims of type <see cref="ApplicationRole.PermissionClaimType"/>.</para>
/// </remarks>
internal sealed class UserRoleService(UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roleManager)
    : IUserRoleService
{
    /// <inheritdoc />
    public async Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        if (!Roles.All.Contains(role)) return AuthErrors.UnknownRole;

        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null) return AuthErrors.UserNotFound;
        if (await users.IsInRoleAsync(user, role)) return Result.Success();

        var added = await users.AddToRoleAsync(user, role);
        return added.Succeeded ? Result.Success() : IdentityErrors.ToError(added);
    }

    /// <inheritdoc />
    /// <remarks>Refuses to remove Admin from the last admin.</remarks>
    public async Task<Result> RemoveFromRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        if (!Roles.All.Contains(role)) return AuthErrors.UnknownRole;

        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null) return AuthErrors.UserNotFound;
        if (!await users.IsInRoleAsync(user, role)) return Result.Success();

        if (role == Roles.Admin && (await users.GetUsersInRoleAsync(Roles.Admin)).Count <= 1)
            return AuthErrors.LastAdmin;

        var removed = await users.RemoveFromRoleAsync(user, role);
        return removed.Succeeded ? Result.Success() : IdentityErrors.ToError(removed);
    }

    /// <inheritdoc />
    /// <remarks>An unknown user has no roles (empty list).</remarks>
    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        return user is null ? [] : (await users.GetRolesAsync(user)).ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Guid>> GetUserIdsInRoleAsync(string role, CancellationToken ct = default)
    {
        if (!Roles.All.Contains(role)) return [];
        return (await users.GetUsersInRoleAsync(role)).Select(u => u.Id).ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        if (!Roles.All.Contains(role)) return false;
        var user = await users.FindByIdAsync(userId.ToString());
        return user is not null && await users.IsInRoleAsync(user, role);
    }

    // ───────────────────────────── Job role permissions ─────────────────────────────

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetRolePermissionsAsync(string role, CancellationToken ct = default)
    {
        if (!Roles.IsJobRole(role)) return [];

        var found = await roleManager.FindByNameAsync(role);
        if (found is null) return [];

        var held = (await roleManager.GetClaimsAsync(found))
            .Where(c => c.Type == ApplicationRole.PermissionClaimType)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

        return Permissions.All.Where(held.Contains).ToList();   // known ones, in the usual order
    }

    /// <inheritdoc />
    /// <remarks>Adds what's missing and removes what's no longer wanted, one claim at a time (Identity saves each).
    /// Unknown permissions already stored are removed too.</remarks>
    public async Task<Result> SetRolePermissionsAsync(string role, IReadOnlyCollection<string> permissions,
                                                      CancellationToken ct = default)
    {
        if (!Roles.IsJobRole(role)) return StaffErrors.NotAJobRole;
        if (permissions.Any(p => !Permissions.IsKnown(p))) return StaffErrors.UnknownPermission;

        var found = await roleManager.FindByNameAsync(role);
        if (found is null) return StaffErrors.NotAJobRole;

        var wanted = permissions.ToHashSet(StringComparer.Ordinal);
        var current = (await roleManager.GetClaimsAsync(found))
            .Where(c => c.Type == ApplicationRole.PermissionClaimType)
            .ToList();

        foreach (var claim in current.Where(c => !wanted.Contains(c.Value)))
        {
            var removed = await roleManager.RemoveClaimAsync(found, claim);
            if (!removed.Succeeded) return IdentityErrors.ToError(removed);
        }

        var have = current.Select(c => c.Value).ToHashSet(StringComparer.Ordinal);
        foreach (var permission in wanted.Where(p => !have.Contains(p)))
        {
            var added = await roleManager.AddClaimAsync(found,
                                                        new Claim(ApplicationRole.PermissionClaimType, permission));
            if (!added.Succeeded) return IdentityErrors.ToError(added);
        }

        return Result.Success();
    }
}
