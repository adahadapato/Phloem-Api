using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>
/// The signed-in user ("me"). The app loads this into SessionService.CurrentUser;
/// Account, Edit profile, Change password, Notifications and Delivery address all read from it.
/// Roles (IsAdmin) come from the login tokens, not from here.
/// </summary>
public sealed record ProfileDto(
    Guid Id,
    string FullName,
    string Email,
    bool EmailConfirmed,
    string? PhoneNumber,
    Country Country,
    Currency Currency,
    string? PhotoUrl,
    DateTime MemberSince,
    bool HasShop,
    bool NotifyOrderUpdates,
    bool NotifyShipmentUpdates,
    bool NotifyOffers,
    bool NotifyNewDishes,
    bool NotifyShopOrders,
    IReadOnlyList<AddressDto> Addresses)
{
    public static ProfileDto From(UserProfile profile, AccountContact contact, string? photoUrl, bool hasShop) => new(
        profile.Id,
        profile.FullName,
        contact.Email,
        contact.EmailConfirmed,
        contact.PhoneNumber,
        profile.Country,
        profile.Country.ToCurrency(),
        photoUrl,
        profile.CreatedAt,
        hasShop,
        profile.NotifyOrderUpdates,
        profile.NotifyShipmentUpdates,
        profile.NotifyOffers,
        profile.NotifyNewDishes,
        profile.NotifyShopOrders,
        profile.Addresses
               .OrderByDescending(a => a.IsDefault)
               .Select(AddressDto.From)
               .ToList());
}