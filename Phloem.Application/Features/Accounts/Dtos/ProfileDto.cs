using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>
/// The Account page: who I am, my personal details, my delivery addresses and my notification switches.
/// </summary>
/// <remarks><c>HasPersonalDetails</c> false = the app shows "Add your date of birth and home address" before opening a
/// shop or sending a parcel.</remarks>
public sealed record ProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    bool EmailConfirmed,
    string? PhoneNumber,
    Country Country,
    Currency Currency,
    string? PhotoUrl,
    DateTime MemberSince,
    bool HasShop,
    DateOnly? DateOfBirth,
    HomeAddressDto? HomeAddress,
    bool HasPersonalDetails,
    bool NotifyOrderUpdates,
    bool NotifyShipmentUpdates,
    bool NotifyOffers,
    bool NotifyNewDishes,
    bool NotifyShopOrders,
    IReadOnlyList<AddressDto> Addresses)
{
    /// <summary>Where I stand with the Terms and the Privacy Policy (filled by <c>GET /api/me</c>).</summary>
    public TermsStatusDto? Terms { get; init; }

    public static ProfileDto From(UserProfile profile, AccountContact contact, string? photoUrl, bool hasShop) => new(
        profile.Id,
        profile.FirstName,
        profile.LastName,
        profile.GetFullName(),
        contact.Email,
        contact.EmailConfirmed,
        contact.PhoneNumber,
        profile.Country,
        profile.Country.ToCurrency(),
        photoUrl,
        profile.CreatedAt,
        hasShop,
        profile.DateOfBirth,
        profile.ResidentialAddress is null ? null : HomeAddressDto.From(profile.ResidentialAddress),
        profile.HasPersonalDetails,
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