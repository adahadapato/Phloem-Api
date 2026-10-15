namespace Phloem.Domain.Enums;

public static class CountryExtensions
{
    public static Currency ToCurrency(this Country country) => country switch
    {
        Country.Ghana         => Currency.GHS,
        Country.Nigeria       => Currency.NGN,
        Country.UnitedKingdom => Currency.GBP,
        _ => throw new ArgumentOutOfRangeException(nameof(country))
    };

    public static string DisplayName(this Country country) => country switch
    {
        Country.UnitedKingdom => "United Kingdom",
        _ => country.ToString()
    };

    public static string Symbol(this Currency currency) => currency switch
    {
        Currency.GHS => "GH₵",
        Currency.NGN => "₦",
        Currency.GBP => "£",
        _ => currency.ToString()
    };
}
