using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Handles <see cref="SetUserRoleCommand"/>.</summary>
/// <remarks>
/// <para><b>Admin</b> is only given or taken by an Admin. Staff the owners give <c>staff.manage</c> can handle job
/// roles and Courier, but never Admin, never their own roles, and only for people whose home country they cover.</para>
/// <para><b>Countries:</b> someone given their first job role starts with their profile's home country, so they can
/// work straight away; the owners can widen it on the Staff page. Someone whose last job role is taken loses their
/// countries too.</para>
/// <para><b>Audit:</b> every change is written to the staff audit log in the same save.</para>
/// </remarks>
public sealed class SetUserRoleCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IUserRoleService roles,
    IRefreshTokenRepository refreshTokens,
    IStaffCountryRepository staffCountries,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<SetUserRoleCommandHandler> logger)
    : IRequestHandler<SetUserRoleCommand, Result>
{
    /// <returns>Success; or <see cref="AuthErrors.RoleNotAssignable"/>, <see cref="UserProfileErrors.NotFound"/>,
    /// <see cref="StaffErrors.OnlyAdminCanChangeAdmin"/>, <see cref="StaffErrors.CannotChangeOwnAccess"/>,
    /// <see cref="AuthErrors.CannotRemoveOwnAdmin"/>,
    /// <see cref="AuthErrors.CannotMakeSuspendedAdmin"/>, <see cref="AuthErrors.LastAdmin"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(SetUserRoleCommand command, CancellationToken ct)
    {
        var actorId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in admin.");

        var role = Roles.Assignable.FirstOrDefault(r => r.Equals(command.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null) return AuthErrors.RoleNotAssignable;

        var actorIsAdmin = currentUser.IsInRole(Roles.Admin);
        if (role == Roles.Admin && !actorIsAdmin) return StaffErrors.OnlyAdminCanChangeAdmin;

        // Staff with staff.manage (not owners) can't change their own roles
        if (!actorIsAdmin && command.UserId == actorId) return StaffErrors.CannotChangeOwnAccess;

        // ...and only for people in their countries (others answer "not found")
        var profile = await profiles.GetByIdAsync(command.UserId, ct);
        if (profile is null || !currentUser.CoversCountry(profile.Country)) return UserProfileErrors.NotFound;

        var target = $"{profile.GetFullName()} ({command.UserId})";
        var now = time.GetUtcNow().UtcDateTime;

        // ───────────────────────────── Give ─────────────────────────────
        if (command.Grant)
        {
            if (role == Roles.Admin && profile.IsSuspendedAt(now)) return AuthErrors.CannotMakeSuspendedAdmin;

            var hadIt = await roles.IsInRoleAsync(command.UserId, role, ct);
            var added = await roles.AddToRoleAsync(command.UserId, role, ct);
            if (added.IsFailure || hadIt) return added;

            // First job role: start with the home country
            string? countryNote = null;
            if (Roles.IsJobRole(role) && !await staffCountries.HasAnyAsync(command.UserId, ct))
            {
                staffCountries.Add(StaffCountry.ForCountry(command.UserId, profile.Country));
                countryNote = $"; covers {profile.Country}";
            }

            audit.Add(StaffAuditEntry.Create(actorId, StaffAction.RoleGiven, target,
                                             detail: $"Role: {Roles.DisplayName(role)}{countryNote}"));
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogWarning("{ActorId} gave {UserId} the {Role} role.", actorId, command.UserId, role);
            return Result.Success();
        }

        // ───────────────────────────── Take ─────────────────────────────
        if (role == Roles.Admin && command.UserId == actorId) return AuthErrors.CannotRemoveOwnAdmin;

        var rolesBefore = await roles.GetRolesAsync(command.UserId, ct);
        var hadRole = rolesBefore.Contains(role, StringComparer.OrdinalIgnoreCase);
        var removed = await roles.RemoveFromRoleAsync(command.UserId, role, ct);   // refuses the last admin
        if (removed.IsFailure || !hadRole) return removed;

        // Last job role gone: their countries go too
        var stillStaff = rolesBefore.Any(r => Roles.IsJobRole(r) && !r.Equals(role, StringComparison.OrdinalIgnoreCase));
        if (Roles.IsJobRole(role) && !stillStaff)
            foreach (var row in await staffCountries.GetForUserToChangeAsync(command.UserId, ct))
                staffCountries.Remove(row);

        // Signed out everywhere, so the old role stops working now rather than at the next token refresh
        // (not when someone drops a role of their own: they'd sign themselves out)
        var signOut = command.UserId != actorId;
        if (signOut)
            foreach (var token in await refreshTokens.GetActiveForUserAsync(command.UserId, now, ct))
                token.Revoke(now);

        audit.Add(StaffAuditEntry.Create(actorId, StaffAction.RoleRemoved, target,
                                         detail: $"Role: {Roles.DisplayName(role)}"));
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogWarning("{ActorId} took the {Role} role from {UserId} (signed out everywhere: {SignedOut}).",
                          actorId, role, command.UserId, signOut);
        return Result.Success();
    }
}
