using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>UserProfiles</c>: the business side of an account (first and last name, country, photo, date of birth,
/// home address, notification switches). Same Id as the Identity user (AspNetUsers), which owns email, phone,
/// password and roles.
/// </summary>
/// <remarks>
/// <para>Owns its <see cref="DeliveryAddress"/> rows: the relationship is configured HERE (the aggregate root),
/// through the private <c>_addresses</c> field, so addresses only change via UserProfile's methods.</para>
/// <para>The home address is stored as columns in this table (<c>ResidentialAddress_Line1</c>…); all null until the
/// user adds it.</para>
/// <para>No database foreign key to AspNetUsers: the Domain doesn't know Identity exists. Both rows are created
/// together in one transaction at sign-up, with the same Id.</para>
/// <para>The full name isn't a column: <c>GetFullName()</c> joins the two parts in code, and the repository's
/// <c>GetFullNamesAsync</c> joins them in SQL for lists.</para>
/// </remarks>
public sealed class UserProfileConfiguration : EntityConfiguration<UserProfile>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        // ---------- Profile ----------
        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(UserProfile.FirstNameMaxLength);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(UserProfile.LastNameMaxLength);
        builder.Property(p => p.Country).IsRequired();                  // enum → int
        builder.Property(p => p.PhotoUrl).HasMaxLength(Url.MaxLength);   // storage path; null = no photo

        // ---------- Personal details ----------
        builder.Property(p => p.DateOfBirth);                           // SQL "date"; null until added

        builder.OwnsOne(p => p.ResidentialAddress, home =>
        {
            home.Property(a => a.Line1).IsRequired().HasMaxLength(ResidentialAddress.Line1MaxLength);
            home.Property(a => a.Line2).HasMaxLength(ResidentialAddress.Line2MaxLength);
            home.Property(a => a.City).IsRequired().HasMaxLength(ResidentialAddress.CityMaxLength);
            home.Property(a => a.RegionOrPostcode).IsRequired()
                .HasMaxLength(ResidentialAddress.RegionOrPostcodeMaxLength);
            home.Property(a => a.Country).IsRequired();
        });

        // ---------- Notification switches ----------
        builder.Property(p => p.NotifyOrderUpdates).IsRequired();
        builder.Property(p => p.NotifyShipmentUpdates).IsRequired();
        builder.Property(p => p.NotifyOffers).IsRequired();
        builder.Property(p => p.NotifyNewDishes).IsRequired();
        builder.Property(p => p.NotifyShopOrders).IsRequired();

        // ---------- Addresses: one profile → many addresses, through the private list ----------
        builder.HasMany(p => p.Addresses)
               .WithOne()
               .HasForeignKey(a => a.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Addresses)
               .HasField("_addresses")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---------- Calculated, not columns ----------
        builder.Ignore(p => p.DefaultAddress);
        builder.Ignore(p => p.HasPersonalDetails);

        // ---------- Lists ----------
        builder.HasIndex(p => new { p.LastName, p.FirstName });   // admin user lists sorted by name
    }
}