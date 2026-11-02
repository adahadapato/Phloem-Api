using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Staff.Dtos;

/// <summary>One person on Admin → Staff: an owner (Admin) or someone with a staff job role.</summary>
/// <param name="Id">The person (same Id as their login).</param>
/// <param name="FullName">"Ama Mensah".</param>
/// <param name="Email">Their login email.</param>
/// <param name="HomeCountry">The country on their profile.</param>
/// <param name="IsAdmin">True for the owners: every permission in every country.</param>
/// <param name="JobRoles">Their job roles (stored names, e.g. "ShipmentManager").</param>
/// <param name="JobRoleNames">The same roles as people read them, e.g. "Parcel hub".</param>
/// <param name="CoversAllCountries">True for admins and staff given "all countries".</param>
/// <param name="Countries">The countries they cover; empty when <paramref name="CoversAllCountries"/>.</param>
/// <param name="TwoFactorEnabled">Whether two-factor sign-in is on (required for all staff).</param>
/// <param name="LastSignIn">When they last signed in (UTC); null = never.</param>
/// <param name="IsSuspended">True while suspended.</param>
/// <param name="IsYou">True for the person looking at the list (they can't change their own access).</param>
public sealed record StaffMemberDto(
    Guid Id,
    string FullName,
    string Email,
    Country HomeCountry,
    bool IsAdmin,
    IReadOnlyList<string> JobRoles,
    IReadOnlyList<string> JobRoleNames,
    bool CoversAllCountries,
    IReadOnlyList<Country> Countries,
    bool TwoFactorEnabled,
    DateTime? LastSignIn,
    bool IsSuspended,
    bool IsYou);
