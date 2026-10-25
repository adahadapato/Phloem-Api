using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>A saved delivery address.</summary>
/// <param name="RecipientName">Who receives the food; null = the account holder (show their name).</param>
/// <param name="PhoneNumber">Who the seller calls, e.g. "+447700900123"; null = the account holder's phone.</param>
public sealed record AddressDto(
    Guid Id,
    string Label,
    string FullAddress,
    Country Country,
    bool IsDefault,
    string? RecipientName,
    string? PhoneNumber)
{
    public static AddressDto From(DeliveryAddress address) => new(
        address.Id,
        address.Label,
        address.FullAddress,
        address.Country,
        address.IsDefault,
        address.RecipientName,
        address.PhoneNumber?.Value);
}