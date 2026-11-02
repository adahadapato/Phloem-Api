using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One country a staff member covers. A staff member has one row per country, or a single "all countries" row.
/// </summary>
/// <remarks>
/// <para><b>Who has rows:</b> people with a staff job role (<see cref="Constants.Roles.JobRoles"/>). Admins don't
/// need any: they cover every country. Customers, sellers and couriers never have rows.</para>
/// <para><b>All countries</b> is stored as <see cref="Country"/> = null, so a country Phloem adds later is covered
/// automatically.</para>
/// <para><b>Checked on every request:</b> the countries travel in the sign-in token; endpoints that know a record's
/// country refuse records outside them (as "not found"), and endpoints that don't know it yet need all countries.</para>
/// <para>Removing a country deletes the row (it isn't soft-deleted); the audit log keeps the history.</para>
/// </remarks>
public class StaffCountry : Entity<Guid>
{
    /// <summary>For EF Core.</summary>
    protected StaffCountry() { }

    private StaffCountry(Guid id) : base(id) { }

    /// <summary>The staff member (same Id as their UserProfile and login).</summary>
    public Guid UserId { get; private set; }

    /// <summary>The country covered; null = every country.</summary>
    public Country? Country { get; private set; }

    /// <summary>True when this row covers every country.</summary>
    public bool IsAllCountries => Country is null;

    /// <summary>Makes a row for one country.</summary>
    /// <param name="userId">The staff member.</param>
    /// <param name="country">The country they cover.</param>
    /// <returns>The new row (not yet saved).</returns>
    /// <exception cref="ArgumentException">No user.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown country.</exception>
    public static StaffCountry ForCountry(Guid userId, Country country)
    {
        if (!Enum.IsDefined(country)) throw new ArgumentOutOfRangeException(nameof(country));
        return Create(userId, country);
    }

    /// <summary>Makes the "every country" row.</summary>
    /// <param name="userId">The staff member.</param>
    /// <returns>The new row (not yet saved).</returns>
    /// <exception cref="ArgumentException">No user.</exception>
    public static StaffCountry ForAllCountries(Guid userId) => Create(userId, null);

    private static StaffCountry Create(Guid userId, Country? country)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A user is required.", nameof(userId));

        return new StaffCountry(Guid.CreateVersion7())
        {
            UserId = userId,
            Country = country
        };
    }
}
