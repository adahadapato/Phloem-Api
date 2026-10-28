using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Admins giving / taking roles from OTHER users, e.g. Seller when a shop is approved.
/// Separate from IAccountService, which is only about the signed-in user's own account.
/// Implemented in Infrastructure using ASP.NET Core Identity.
/// </summary>
public interface IUserRoleService
{
    /// <summary>Use Roles constants: AddToRoleAsync(ownerId, Roles.Seller).</summary>
    Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<Result> RemoveFromRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Everyone who has a role, e.g. all couriers (to offer them in a "Choose a courier" list).</summary>
    /// <param name="role">A <see cref="Constants.Roles"/> name.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>Their user Ids; empty for an unknown role.</returns>
    Task<IReadOnlyCollection<Guid>> GetUserIdsInRoleAsync(string role, CancellationToken ct = default);

    /// <summary>Does this user have this role right now (from the database, not the token)?</summary>
    /// <param name="userId">The user.</param>
    /// <param name="role">A <see cref="Constants.Roles"/> name.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>False for an unknown user or role.</returns>
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default);
}