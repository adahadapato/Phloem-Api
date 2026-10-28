using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Deliveries.Common;

/// <summary>
/// Who can deliver: people with the <see cref="Roles.Courier"/> role, living in the delivery's country, not suspended
/// and not deleted. Used by the "Choose a courier" lists and by every command that gives something to a courier.
/// </summary>
/// <remarks>The role is checked in the DATABASE (not a token), so a courier whose role was just removed can't be
/// chosen.</remarks>
public sealed class CourierDirectory(IUserRoleService roles, IUserProfileRepository profiles, TimeProvider time)
{
    /// <summary>The couriers who can deliver in a country, by name.</summary>
    /// <param name="country">The delivery's country.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Their profiles (read-only); empty when there are none.</returns>
    public async Task<IReadOnlyList<UserProfile>> ListAsync(Country country, CancellationToken ct)
    {
        var ids = await roles.GetUserIdsInRoleAsync(Roles.Courier, ct);
        var people = await profiles.GetInCountryAsync(ids, country, ct);

        var now = time.GetUtcNow().UtcDateTime;
        return people.Where(p => !p.IsSuspendedAt(now)).ToList();
    }

    /// <summary>Checks one chosen courier can deliver in this country.</summary>
    /// <param name="courierId">The chosen courier.</param>
    /// <param name="country">The delivery's country.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Their profile; or <see cref="DeliveryErrors.CourierNotFound"/>,
    /// <see cref="DeliveryErrors.CourierInAnotherCountry"/>.</returns>
    public async Task<Result<UserProfile>> FindAsync(Guid courierId, Country country, CancellationToken ct)
    {
        if (!await roles.IsInRoleAsync(courierId, Roles.Courier, ct)) return DeliveryErrors.CourierNotFound;

        var profile = await profiles.GetByIdAsync(courierId, ct);       // deleted accounts are filtered out
        if (profile is null || profile.IsSuspendedAt(time.GetUtcNow().UtcDateTime))
            return DeliveryErrors.CourierNotFound;

        if (profile.Country != country) return DeliveryErrors.CourierInAnotherCountry;
        return profile;
    }
}
