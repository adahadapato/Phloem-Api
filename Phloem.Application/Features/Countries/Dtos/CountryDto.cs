using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Countries.Dtos;

/// <summary>
/// One country for the app's and website's country pickers: "🇬🇭 Ghana · GH₵".
/// Country is the value to SEND back to the API; everything else is for showing.
/// </summary>
/// <param name="Country">The value the API expects, e.g. "UnitedKingdom".</param>
/// <param name="Name">The name people read, e.g. "United Kingdom".</param>
/// <param name="IsoCode">Two-letter ISO code, e.g. "GB" (to guess the user's country from the phone's region).</param>
/// <param name="Flag">Flag emoji, e.g. 🇬🇧.</param>
/// <param name="Currency">The country's currency, e.g. "GBP".</param>
/// <param name="CurrencySymbol">e.g. "£".</param>
/// <param name="CanSendParcels">True when at least one shipping route FROM this country is switched on.</param>
/// <param name="SendsParcelsTo">Countries a parcel can be sent to from here (switched-on routes only).</param>
public sealed record CountryDto(
    Country Country,
    string Name,
    string IsoCode,
    string Flag,
    Currency Currency,
    string CurrencySymbol,
    bool CanSendParcels,
    IReadOnlyList<Country> SendsParcelsTo)
{
    /// <summary>Builds one country; <paramref name="destinations"/> come from the switched-on routes.</summary>
    public static CountryDto From(Country country, IReadOnlyList<Country> destinations)
    {
        var currency = country.ToCurrency();
        return new CountryDto(
            country,
            country.DisplayName(),
            country.IsoCode(),
            country.Flag(),
            currency,
            currency.Symbol(),
            destinations.Count > 0,
            destinations);
    }
}
