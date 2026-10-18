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
/// </remarks>
internal sealed class UserRoleService(UserManager<ApplicationUser> users) : IUserRoleService
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
}
