namespace Phloem.Presentation.Settings.Authorization;

/// <summary>
/// How an endpoint that needs a staff permission deals with countries.
/// </summary>
/// <remarks>
/// <para><b>Safe default:</b> an endpoint whose handler doesn't filter by country yet uses
/// <see cref="AllCountries"/>, so a one-country staff member can't reach other countries' records through it. When a
/// handler learns to filter (or refuse records outside the person's countries), its endpoint switches to
/// <see cref="RecordCountry"/>.</para>
/// </remarks>
public enum CountryRule
{
    /// <summary>The person must cover every country (Admins, and staff given "all countries").</summary>
    AllCountries = 0,

    /// <summary>The person must cover at least one country; the handler checks each record's country.</summary>
    RecordCountry = 1,

    /// <summary>The records have no country (e.g. Contact us messages), so countries aren't checked.</summary>
    NotCountrySpecific = 2
}
