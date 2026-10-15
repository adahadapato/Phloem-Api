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
}