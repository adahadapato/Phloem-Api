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

/// <summary>Handles <see cref="SetStaffCountriesCommand"/>.</summary>
public sealed class SetStaffCountriesCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IUserRoleService roles,
    IStaffCountryRepository staffCountries,
    IRefreshTokenRepository refreshTokens,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<SetStaffCountriesCommandHandler> logger)
    : IRequestHandler<SetStaffCountriesCommand, Result>
{
    /// <summary>Replaces the countries the person covers (within what the actor covers) and ends their sessions.</summary>
    /// <param name="command">Who and which countries.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="StaffErrors.CannotChangeOwnAccess"/>, <see cref="UserProfileErrors.NotFound"/>,
    /// <see cref="StaffErrors.NotStaff"/>, <see cref="StaffErrors.CountryNotCovered"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(SetStaffCountriesCommand command, CancellationToken ct)
    {
        var actorId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");
        if (command.UserId == actorId) return StaffErrors.CannotChangeOwnAccess;

        // 1. The person: must exist, be visible to the actor, and hold a job role
        var profile = await profiles.GetByIdAsync(command.UserId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var current = await staffCountries.GetForUserToChangeAsync(command.UserId, ct);
        var currentAll = current.Any(r => r.IsAllCountries);
        var currentCountries = current.Where(r => !r.IsAllCountries).Select(r => r.Country!.Value).ToHashSet();

        var visible = currentUser.CoversAllCountries
                      || currentUser.CoversCountry(profile.Country)
                      || currentCountries.Any(currentUser.CoversCountry);
        if (!visible) return UserProfileErrors.NotFound;

        var personRoles = await roles.GetRolesAsync(command.UserId, ct);
        if (!personRoles.Any(Roles.IsJobRole)) return StaffErrors.NotStaff;

        // 2. What the actor may change
        var requested = (command.Countries ?? []).Distinct().ToHashSet();
        if (!currentUser.CoversAllCountries)
        {
            if (command.AllCountries || currentAll) return StaffErrors.CountryNotCovered;
            if (requested.Any(c => !currentUser.CoversCountry(c))) return StaffErrors.CountryNotCovered;

            // Countries the actor doesn't cover stay as they were
            requested.UnionWith(currentCountries.Where(c => !currentUser.CoversCountry(c)));
        }

        // 3. Change only what differs (removing and re-adding the same row in one save could trip the unique index)
        foreach (var row in current)
        {
            var keep = command.AllCountries
                ? row.IsAllCountries
                : !row.IsAllCountries && requested.Contains(row.Country!.Value);
            if (!keep) staffCountries.Remove(row);
        }

        if (command.AllCountries && !currentAll)
            staffCountries.Add(StaffCountry.ForAllCountries(command.UserId));
        else if (!command.AllCountries)
            foreach (var country in requested.Where(c => !currentCountries.Contains(c)).Order())
                staffCountries.Add(StaffCountry.ForCountry(command.UserId, country));

        // 4. Signed out everywhere, so the change applies now
        var now = time.GetUtcNow().UtcDateTime;
        foreach (var token in await refreshTokens.GetActiveForUserAsync(command.UserId, now, ct))
            token.Revoke(now);

        // 5. Audit (same save)
        var before = currentAll ? "All" : Describe(currentCountries);
        var after = command.AllCountries ? "All" : Describe(requested);
        audit.Add(StaffAuditEntry.Create(actorId, StaffAction.CountriesChanged,
                                         $"{profile.GetFullName()} ({command.UserId})", profile.Country,
                                         $"From {before} to {after}"));

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogWarning("{ActorId} changed the countries of {UserId} from {Before} to {After}.",
                          actorId, command.UserId, before, after);
        return Result.Success();
    }

    /// <summary>"Ghana, Nigeria", or "none".</summary>
    private static string Describe(IEnumerable<Country> countries)
    {
        var list = countries.Order().Select(c => c.ToString()).ToList();
        return list.Count == 0 ? "none" : string.Join(", ", list);
    }
}
