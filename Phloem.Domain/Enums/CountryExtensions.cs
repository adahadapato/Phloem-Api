namespace Phloem.Domain.Enums;

/// <summary>
/// Everything that belongs to a country: its currency, how people see its name, its ISO code and flag.
/// The ONE place these live: the API serves them to the app and the website (<c>GET /api/countries</c>),
/// so no client keeps its own list.
/// </summary>
public static class CountryExtensions
{
    /// <summary>The country's currency (every price in the country uses it).</summary>
    /// <exception cref="ArgumentOutOfRangeException">A country without a currency (add it here).</exception>
    public static Currency ToCurrency(this Country country) => country switch
    {
        Country.Ghana         => Currency.GHS,
        Country.Nigeria       => Currency.NGN,
        Country.UnitedKingdom => Currency.GBP,
        _ => throw new ArgumentOutOfRangeException(nameof(country))
    };

    /// <summary>The name people read, e.g. UnitedKingdom → "United Kingdom".</summary>
    public static string DisplayName(this Country country) => country switch
    {
        Country.UnitedKingdom => "United Kingdom",
        _ => country.ToString()
    };

    /// <summary>The currency's symbol, e.g. "GH₵", "₦", "£".</summary>
    public static string Symbol(this Currency currency) => currency switch
    {
        Currency.GHS => "GH₵",
        Currency.NGN => "₦",
        Currency.GBP => "£",
        _ => currency.ToString()
    };

    /// <summary>The two-letter ISO 3166 code, e.g. "GH", "NG", "GB" (phones use it to guess the user's country).</summary>
    /// <exception cref="ArgumentOutOfRangeException">A country without a code (add it here).</exception>
    public static string IsoCode(this Country country) => country switch
    {
        Country.Ghana         => "GH",
        Country.Nigeria       => "NG",
        Country.UnitedKingdom => "GB",
        _ => throw new ArgumentOutOfRangeException(nameof(country))
    };

    /// <summary>
    /// The flag emoji, e.g. 🇬🇭. Worked out from <see cref="IsoCode"/> (each letter becomes a "regional indicator"
    /// symbol, and phones draw the pair as a flag), so a new country gets its flag with no extra code.
    /// </summary>
    public static string Flag(this Country country)
    {
        const int regionalIndicatorA = 0x1F1E6;
        return string.Concat(country.IsoCode().Select(letter => char.ConvertFromUtf32(regionalIndicatorA + letter - 'A')));
    }
}
