using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>A home address as the app sends it (sign-up and the personal-details page).</summary>
/// <param name="Line1">House / flat and street.</param>
/// <param name="Line2">More of the address (optional).</param>
/// <param name="City">Town or city.</param>
/// <param name="RegionOrPostcode">Postcode, state or region.</param>
/// <param name="Country">The country.</param>
public sealed record HomeAddressInput(string Line1, string? Line2, string City, string RegionOrPostcode,
                                      Country Country)
{
    /// <summary>The Domain value object (the validator has already checked every part).</summary>
    public ResidentialAddress ToResidentialAddress() =>
        ResidentialAddress.Create(Line1, Line2, City, RegionOrPostcode, Country);
}