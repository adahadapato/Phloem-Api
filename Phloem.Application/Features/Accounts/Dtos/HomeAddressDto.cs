using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>A home address as the app shows it, with a ready-made single line.</summary>
public sealed record HomeAddressDto(string Line1, string? Line2, string City, string RegionOrPostcode,
                                    Country Country, string SingleLine)
{
    /// <summary>Maps the Domain value object.</summary>
    public static HomeAddressDto From(ResidentialAddress address) => new(
        address.Line1, address.Line2, address.City, address.RegionOrPostcode, address.Country, address.ToSingleLine());
}