using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>Dishes</c>: a shop's dish (a catalog name + optional detail, price, availability), with its photos in
/// <c>DishPhotos</c> (configured here, through the private <c>_photos</c> field).
/// </summary>
/// <remarks>
/// <para><b>Soft delete:</b> deleted dishes are hidden by the base filter but their rows stay, because order lines
/// point at them; their photo files are never deleted either.</para>
/// <para><b>Snapshot columns:</b> <c>Name</c>, <c>Category</c>, <c>Currency</c> and <c>Country</c> are copied from the
/// catalog and the shop, so buyer searches need no joins.</para>
/// <para><b>Search:</b> buyers' searches filter on country, category, availability and <c>NeedsPriceUpdate</c>
/// (dishes outside a new price range are hidden until the seller fixes the price), hence the index.</para>
/// </remarks>
public sealed class DishConfiguration : EntityConfiguration<Dish>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<Dish> builder)
    {
        builder.ToTable("Dishes");

        // ---------- Shop and catalog name ----------
        builder.HasOne(d => d.Vendor)
               .WithMany()
               .HasForeignKey(d => d.VendorId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);      // shops are soft-deleted (closed), never removed

        builder.HasOne(d => d.DishCatalog)
               .WithMany()
               .HasForeignKey(d => d.DishCatalogId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);      // a name in use can't be removed

        // ---------- Details ----------
        builder.Property(d => d.Detail).HasMaxLength(Dish.DetailMaxLength);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(Dish.NameMaxLength);
        builder.Property(d => d.Category).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(Dish.DescriptionMaxLength);

        // ---------- Price ----------
        builder.Property(d => d.Price).IsRequired().HasMoneyPrecision();
        builder.Property(d => d.Currency).IsRequired();
        builder.Property(d => d.Country).IsRequired();
        builder.Property(d => d.NeedsPriceUpdate).IsRequired();

        // ---------- Availability & reputation ----------
        builder.Property(d => d.IsAvailable).IsRequired();
        builder.Property(d => d.Rating).IsRequired().HasPrecision(3, 2);   // 0.00–5.00
        builder.Property(d => d.RatingCount).IsRequired();

        // ---------- Photos ----------
        builder.HasMany(d => d.Photos)
               .WithOne()
               .HasForeignKey(p => p.DishId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.Photos)
               .HasField("_photos")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---------- Calculated, not columns ----------
        builder.Ignore(d => d.PrimaryPhoto);
        builder.Ignore(d => d.CanBeOrdered);

        // ---------- Lists ----------
        builder.HasIndex(d => d.VendorId);                                                       // a shop's dishes
        builder.HasIndex(d => new { d.DishCatalogId, d.Country });                               // price-range changes
        builder.HasIndex(d => new { d.Country, d.Category, d.IsAvailable, d.NeedsPriceUpdate }); // buyer search
    }
}