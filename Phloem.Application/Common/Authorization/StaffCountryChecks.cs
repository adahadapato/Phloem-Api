using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Common.Authorization;

/// <summary>
/// The country checks staff handlers share, so every handler applies the same rule.
/// </summary>
/// <remarks>
/// <para><b>Not covered = not found:</b> a handler that loads a record outside the person's countries answers as if the
/// record didn't exist (same as ownership checks), so staff can't learn what exists in other countries.</para>
/// <para><b>Parcels</b> belong to two countries: staff covering either the origin (receiving and weighing at the hub)
/// or the destination (arrival and delivery) can work on them.</para>
/// </remarks>
public static class StaffCountryChecks
{
    /// <summary>True when the person may see and work on this parcel.</summary>
    /// <param name="currentUser">The signed-in staff member or admin.</param>
    /// <param name="shipment">The parcel.</param>
    /// <returns>True when they cover its origin or destination country.</returns>
    public static bool CoversParcel(this ICurrentUser currentUser, Shipment shipment) =>
        currentUser.CoversCountry(shipment.FromCountry) || currentUser.CoversCountry(shipment.ToCountry);

    /// <summary>
    /// The countries to limit a list to: null when the person covers every country (no limit), otherwise their
    /// countries (possibly empty, which matches nothing).
    /// </summary>
    /// <param name="currentUser">The signed-in staff member or admin.</param>
    /// <returns>Null for "no limit", or the countries to keep.</returns>
    public static IReadOnlyCollection<Country>? CountryLimit(this ICurrentUser currentUser) =>
        currentUser.CoversAllCountries ? null : currentUser.Countries;
}
