namespace Phloem.Domain.ValueObjects;

/// <summary>
/// Where a person LIVES (not where food is delivered): one per profile, structured so it can be checked and used on
/// courier and customs forms.
/// </summary>
/// <remarks>
/// <para>Required (with date of birth) before opening a shop or sending a parcel; optional otherwise.</para>
/// <para>Sizes are chosen so <see cref="ToSingleLine"/> always fits a delivery address (300 characters).</para>
/// <para>EF Core builds it through the private parameterless constructor and fills the properties.</para>
/// </remarks>
public sealed class ResidentialAddress : ValueObject
{
    /// <summary>Longest first line, e.g. "Flat 31, Mar House".</summary>
    public const int Line1MaxLength = 80;

    /// <summary>Longest second line, e.g. "50 The Hyde".</summary>
    public const int Line2MaxLength = 80;

    /// <summary>Longest town or city.</summary>
    public const int CityMaxLength = 80;

    /// <summary>Longest region / state / postcode, e.g. "NW9 6LH", "Lagos State", "Greater Accra".</summary>
    public const int RegionOrPostcodeMaxLength = 50;

    /// <summary>Longest single-line form: the four parts joined with ", " (296).</summary>
    public const int SingleLineMaxLength =
        Line1MaxLength + 2 + Line2MaxLength + 2 + CityMaxLength + 2 + RegionOrPostcodeMaxLength;

    /// <summary>Required by EF Core; never call from code.</summary>
    private ResidentialAddress()
    {
        Line1 = string.Empty;
        City = string.Empty;
        RegionOrPostcode = string.Empty;
    }

    private ResidentialAddress(string line1, string? line2, string city, string regionOrPostcode, Enums.Country country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        RegionOrPostcode = regionOrPostcode;
        Country = country;
    }

    /// <summary>House / flat and street, e.g. "Flat 31, Mar House".</summary>
    public string Line1 { get; }

    /// <summary>More of the address, if needed; null when blank.</summary>
    public string? Line2 { get; }

    /// <summary>Town or city.</summary>
    public string City { get; }

    /// <summary>UK postcode, Nigerian state or Ghanaian region.</summary>
    public string RegionOrPostcode { get; }

    /// <summary>The country.</summary>
    public Enums.Country Country { get; }

    /// <summary>Checks and tidies an address (trims; blank Line2 becomes null).</summary>
    /// <param name="line1">House / flat and street.</param>
    /// <param name="line2">More of the address (optional).</param>
    /// <param name="city">Town or city.</param>
    /// <param name="regionOrPostcode">Postcode, state or region.</param>
    /// <param name="country">The country.</param>
    /// <returns>The address.</returns>
    /// <exception cref="ArgumentException">A required part is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A part is too long, or the country isn't valid.</exception>
    public static ResidentialAddress Create(string line1, string? line2, string city, string regionOrPostcode,
                                            Enums.Country country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(line1);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(regionOrPostcode);
        if (!Enum.IsDefined(country)) throw new ArgumentOutOfRangeException(nameof(country));

        var first = line1.Trim();
        var second = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        var town = city.Trim();
        var region = regionOrPostcode.Trim();

        ArgumentOutOfRangeException.ThrowIfGreaterThan(first.Length, Line1MaxLength, nameof(line1));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(second?.Length ?? 0, Line2MaxLength, nameof(line2));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(town.Length, CityMaxLength, nameof(city));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(region.Length, RegionOrPostcodeMaxLength, nameof(regionOrPostcode));

        return new ResidentialAddress(first, second, town, region, country);
    }

    /// <summary>"Flat 31, Mar House, 50 The Hyde, London, NW9 6LH": for delivery addresses and display.</summary>
    /// <returns>The parts joined with ", " (blank Line2 left out).</returns>
    public string ToSingleLine() =>
        string.Join(", ", new[] { Line1, Line2, City, RegionOrPostcode }.Where(p => !string.IsNullOrWhiteSpace(p)));

    /// <inheritdoc />
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return RegionOrPostcode;
        yield return Country;
    }
}