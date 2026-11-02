using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>StaffCountries</c>: which countries each staff member covers.</summary>
/// <remarks>
/// <para><b>One row per (person, country):</b> unique index on (UserId, Country). <c>HasFilter(null)</c> turns off
/// EF's usual "only when not null" filter, so the "all countries" row (Country = null) can't be added twice
/// either.</para>
/// <para>Rows are deleted (not soft-deleted) when a country is taken away, so the unique index never trips over an
/// old row.</para>
/// </remarks>
public sealed class StaffCountryConfiguration : EntityConfiguration<StaffCountry>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<StaffCountry> builder)
    {
        builder.ToTable("StaffCountries");

        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.Country);   // null = every country

        builder.Ignore(s => s.IsAllCountries);   // calculated

        builder.HasIndex(s => new { s.UserId, s.Country }).IsUnique().HasFilter(null);
    }
}
