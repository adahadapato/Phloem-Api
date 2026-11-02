using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Commands;

/// <summary>Handles <see cref="SetJobRolePermissionsCommand"/>.</summary>
public sealed class SetJobRolePermissionsCommandHandler(
    ICurrentUser currentUser,
    IUserRoleService roles,
    IRefreshTokenRepository refreshTokens,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<SetJobRolePermissionsCommandHandler> logger)
    : IRequestHandler<SetJobRolePermissionsCommand, Result>
{
    /// <summary>Saves the new permission set, logs the difference, and signs members out if anything was taken.</summary>
    /// <param name="command">The role and its new permissions.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="StaffErrors.OnlyAdminCanChangeRolePermissions"/>,
    /// <see cref="StaffErrors.NotAJobRole"/>, <see cref="StaffErrors.UnknownPermission"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(SetJobRolePermissionsCommand command, CancellationToken ct)
    {
        var actorId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in admin.");
        if (!currentUser.IsInRole(Roles.Admin)) return StaffErrors.OnlyAdminCanChangeRolePermissions;

        var role = Roles.JobRoles.FirstOrDefault(r => r.Equals(command.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null) return StaffErrors.NotAJobRole;

        // 1. What changes
        var before = await roles.GetRolePermissionsAsync(role, ct);
        var after = command.Permissions.Distinct(StringComparer.Ordinal).ToList();
        var added = after.Except(before, StringComparer.Ordinal).ToList();
        var removed = before.Except(after, StringComparer.Ordinal).ToList();
        if (added.Count == 0 && removed.Count == 0) return Result.Success();

        // 2. Save the permissions (Identity saves them itself)
        var saved = await roles.SetRolePermissionsAsync(role, after, ct);
        if (saved.IsFailure) return saved;

        // 3. Taken away → members signed out everywhere, so it stops working now
        var now = time.GetUtcNow().UtcDateTime;
        if (removed.Count > 0)
            foreach (var memberId in await roles.GetUserIdsInRoleAsync(role, ct))
                foreach (var token in await refreshTokens.GetActiveForUserAsync(memberId, now, ct))
                    token.Revoke(now);

        // 4. Audit
        var detail = string.Join("; ", new[]
        {
            added.Count > 0 ? "Added: " + string.Join(", ", added) : null,
            removed.Count > 0 ? "Removed: " + string.Join(", ", removed) : null
        }.OfType<string>());
        audit.Add(StaffAuditEntry.Create(actorId, StaffAction.RolePermissionsChanged,
                                         $"Job role {Roles.DisplayName(role)}", detail: detail));

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogWarning("{ActorId} changed the permissions of job role {Role}: {Detail}.", actorId, role, detail);
        return Result.Success();
    }
}
