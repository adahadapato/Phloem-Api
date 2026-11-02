using MediatR;
using Phloem.Application.Features.Staff.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Queries;

/// <summary>Handles <see cref="GetStaffQuery"/>.</summary>
/// <remarks>
/// Staff are few (owners plus a small team), so each person's login details are read one by one; the countries come
/// in one query.
/// </remarks>
public sealed class GetStaffQueryHandler(
    ICurrentUser currentUser,
    IUserRoleService roles,
    IUserProfileRepository profiles,
    IAccountService accounts,
    IStaffCountryRepository staffCountries,
    TimeProvider time)
    : IRequestHandler<GetStaffQuery, Result<IReadOnlyList<StaffMemberDto>>>
{
    /// <summary>Lists the staff the person may see, owners first, then by name.</summary>
    /// <param name="query">The query (no filters).</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The staff list.</returns>
    public async Task<Result<IReadOnlyList<StaffMemberDto>>> Handle(GetStaffQuery query, CancellationToken ct)
    {
        // 1. Who is staff, and with which job roles
        var adminIds = (await roles.GetUserIdsInRoleAsync(Roles.Admin, ct)).ToHashSet();
        var jobRolesOf = new Dictionary<Guid, List<string>>();
        foreach (var role in Roles.JobRoles)
            foreach (var id in await roles.GetUserIdsInRoleAsync(role, ct))
            {
                if (!jobRolesOf.TryGetValue(id, out var list)) jobRolesOf[id] = list = [];
                list.Add(role);
            }

        var ids = adminIds.Union(jobRolesOf.Keys).ToList();
        var rowsOf = await staffCountries.GetForUsersAsync(ids, ct);
        var now = time.GetUtcNow().UtcDateTime;

        // 2. One line per person
        var items = new List<StaffMemberDto>(ids.Count);
        foreach (var id in ids)
        {
            var profile = await profiles.GetByIdAsync(id, ct);
            if (profile is null) continue;   // deleted account

            var isAdmin = adminIds.Contains(id);
            var rows = rowsOf.GetValueOrDefault(id) ?? [];
            var allCountries = isAdmin || rows.Any(r => r.IsAllCountries);
            IReadOnlyList<Country> countries = allCountries
                ? []
                : rows.Select(r => r.Country!.Value).Distinct().Order().ToList();

            // Someone covering only some countries sees owners, and staff who overlap with them
            if (!currentUser.CoversAllCountries && !isAdmin
                && !currentUser.CoversCountry(profile.Country)
                && !(allCountries || countries.Any(currentUser.CoversCountry)))
                continue;

            var jobRoles = jobRolesOf.GetValueOrDefault(id) ?? [];
            var contact = await accounts.GetContactAsync(id, ct);
            var twoFactor = await accounts.GetTwoFactorStatusAsync(id, ct);

            items.Add(new StaffMemberDto(
                id,
                profile.GetFullName(),
                contact.IsSuccess ? contact.Value.Email : string.Empty,
                profile.Country,
                isAdmin,
                jobRoles,
                jobRoles.Select(Roles.DisplayName).ToList(),
                allCountries,
                countries,
                twoFactor.IsSuccess && twoFactor.Value.IsEnabled,
                await accounts.GetLastSignInAsync(id, ct),
                profile.IsSuspendedAt(now),
                id == currentUser.UserId));
        }

        IReadOnlyList<StaffMemberDto> ordered = items.OrderByDescending(s => s.IsAdmin)
                                                     .ThenBy(s => s.FullName, StringComparer.OrdinalIgnoreCase)
                                                     .ToList();
        return Result<IReadOnlyList<StaffMemberDto>>.Success(ordered);
    }
}
