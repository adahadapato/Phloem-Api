using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Handles <see cref="SetUserRoleCommand"/>.</summary>
public sealed class SetUserRoleCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IUserRoleService roles,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<SetUserRoleCommandHandler> logger)
    : IRequestHandler<SetUserRoleCommand, Result>
{
    /// <returns>Success; or <see cref="AuthErrors.RoleNotAssignable"/>, <see cref="UserProfileErrors.NotFound"/>,
    /// <see cref="AuthErrors.CannotRemoveOwnAdmin"/>, <see cref="AuthErrors.CannotMakeSuspendedAdmin"/>,
    /// <see cref="AuthErrors.LastAdmin"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in admin (endpoint misconfigured).</exception>
    public async Task<Result> Handle(SetUserRoleCommand command, CancellationToken ct)
    {
        var adminId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in admin.");

        var role = Roles.Assignable.FirstOrDefault(r => r.Equals(command.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null) return AuthErrors.RoleNotAssignable;

        var profile = await profiles.GetByIdAsync(command.UserId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var now = time.GetUtcNow().UtcDateTime;
        if (command.Grant)
        {
            if (role == Roles.Admin && profile.IsSuspendedAt(now)) return AuthErrors.CannotMakeSuspendedAdmin;

            var added = await roles.AddToRoleAsync(command.UserId, role, ct);
            if (added.IsFailure) return added;

            logger.LogWarning("Admin {AdminId} gave {UserId} the {Role} role.", adminId, command.UserId, role);
            return Result.Success();
        }

        if (role == Roles.Admin && command.UserId == adminId) return AuthErrors.CannotRemoveOwnAdmin;

        var hadIt = (await roles.GetRolesAsync(command.UserId, ct)).Contains(role, StringComparer.OrdinalIgnoreCase);
        var removed = await roles.RemoveFromRoleAsync(command.UserId, role, ct);   // refuses the last admin
        if (removed.IsFailure || !hadIt) return removed;

        // Signed out everywhere, so the old role stops working now rather than at the next token refresh
        // (not when admins drop a staff role of their own: they'd sign themselves out)
        var signOut = command.UserId != adminId;
        if (signOut)
        {
            foreach (var token in await refreshTokens.GetActiveForUserAsync(command.UserId, now, ct))
                token.Revoke(now);
            await unitOfWork.SaveChangesAsync(ct);
        }

        logger.LogWarning("Admin {AdminId} took the {Role} role from {UserId} (signed out everywhere: {SignedOut}).",
                          adminId, role, command.UserId, signOut);
        return Result.Success();
    }
}
