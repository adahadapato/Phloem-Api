using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>One person on Admin → Users: who they are, their roles and shop, and any suspension.</summary>
/// <param name="Id">The person (same Id as their login).</param>
/// <param name="FullName">"Ama Mensah".</param>
/// <param name="Email">Their login email.</param>
/// <param name="PhoneNumber">Their phone, if given.</param>
/// <param name="Country">Where they shop.</param>
/// <param name="Roles">e.g. Customer, Seller, Admin.</param>
/// <param name="MemberSince">When they signed up (UTC).</param>
/// <param name="ShopName">Their shop's name; null when they have none.</param>
/// <param name="IsSuspended">True while suspended.</param>
/// <param name="SuspendedUntil">When the suspension ends (UTC); null = until further notice (or not suspended).</param>
/// <param name="SuspensionReason">Why; null when not suspended.</param>
/// <param name="CanBeSuspended">False for admins (and for yourself): the app hides the Suspend button.</param>
/// <param name="PhotoUrl">Their profile photo's public URL; null when they haven't added one.</param>
/// <param name="IsYou">True for the admin looking at the list (they can't take their own Admin role away).</param>
public sealed record AdminUserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    Country Country,
    IReadOnlyCollection<string> Roles,
    DateTime MemberSince,
    string? ShopName,
    bool IsSuspended,
    DateTime? SuspendedUntil,
    string? SuspensionReason,
    bool CanBeSuspended,
    string? PhotoUrl,
    bool IsYou);
