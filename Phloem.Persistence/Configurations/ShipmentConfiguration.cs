using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Persistence.Converters;
using Phloem.Persistence.Mappings;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>Shipments</c>: one parcel sent between two countries, with its declared items in <c>ShipmentItems</c> and
/// its tracking timeline in <c>ShipmentEvents</c> (both configured here, through the private <c>_items</c> and
/// <c>_timeline</c> fields).
/// </summary>
/// <remarks>
/// <para><b>Snapshots:</b> the sender's name, the route's countries and the recipient's details are COPIED at booking,
/// so the parcel's record never changes if the profile or route does.</para>
/// <para><b>Links:</b> foreign keys to the sender (UserProfile) and the route with Restrict; neither is ever removed.
/// Repositories must NOT <c>Include</c> <c>Sender</c> (a deleted profile is filtered out and would hide the parcel).</para>
/// <para><b>Weights</b> are decimal(9,2) kg; <b>money</b> decimal(18,2) in the origin currency.</para>
/// <para><b>Calculated, not columns:</b> TotalDeclaredValue, IsPaid, NeedsRefund, CanCancel, EstimatedArrival.</para>
/// </remarks>
public sealed class ShipmentConfiguration : EntityConfiguration<Shipment>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        // ---------- Identity ----------
        builder.Property(s => s.TrackingNumber).IsRequired().HasMaxLength(Shipment.TrackingNumberMaxLength);
        builder.HasIndex(s => s.TrackingNumber).IsUnique();

        // ---------- Sender ----------
        builder.HasOne(s => s.Sender)
               .WithMany()
               .HasForeignKey(s => s.SenderId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.SenderName).IsRequired().HasMaxLength(UserProfile.FullNameMaxLength);
        builder.Property(s => s.SenderPhone)
               .IsRequired()
               .HasConversion(new PhoneNumberConverter())
               .HasMaxLength(PhoneNumberConverter.MaxLength);

        // ---------- Route (link + snapshot) ----------
        builder.HasOne<ShipmentRoute>()
               .WithMany()
               .HasForeignKey(s => s.ShipmentRouteId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.FromCountry).IsRequired();
        builder.Property(s => s.ToCountry).IsRequired();

        // ---------- Recipient (snapshot) ----------
        builder.Property(s => s.RecipientName).IsRequired().HasMaxLength(Shipment.RecipientNameMaxLength);
        builder.Property(s => s.RecipientPhone)
               .IsRequired()
               .HasConversion(new PhoneNumberConverter())
               .HasMaxLength(PhoneNumberConverter.MaxLength);
        builder.Property(s => s.RecipientAddressLine).IsRequired().HasMaxLength(Shipment.AddressLineMaxLength);
        builder.Property(s => s.RecipientCity).IsRequired().HasMaxLength(Shipment.CityMaxLength);
        builder.Property(s => s.RecipientRegionOrPostcode).IsRequired().HasMaxLength(Shipment.RegionOrPostcodeMaxLength);

        // ---------- Parcel ----------
        builder.Property(s => s.Speed).IsRequired();
        builder.Property(s => s.EstimatedWeightKg).IsRequired().HasWeightPrecision();
        builder.Property(s => s.WeighedWeightKg).HasWeightPrecision();          // null until weighed
        builder.Property(s => s.TransitDaysMin);                               // null until weighed
        builder.Property(s => s.TransitDaysMax);

        // ---------- Money ----------
        builder.Property(s => s.Currency).IsRequired();
        builder.Property(s => s.EstimatedPrice).IsRequired().HasMoneyPrecision();
        builder.Property(s => s.FinalPrice).HasMoneyPrecision();               // null until weighed
        builder.Property(s => s.PaymentReference).HasMaxLength(Shipment.PaymentReferenceMaxLength);

        // ---------- Status & dates ----------
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.NoProhibitedItemsConfirmedAtUtc).IsRequired();
        builder.Property(s => s.PaidAtUtc);
        builder.Property(s => s.DeliveredAtUtc);
        builder.Property(s => s.CancelledAtUtc);

        // ---------- Items & timeline ----------
        builder.HasMany(s => s.Items)
               .WithOne()
               .HasForeignKey(i => i.ShipmentId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Items)
               .HasField("_items")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Timeline)
               .WithOne()
               .HasForeignKey(e => e.ShipmentId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Timeline)
               .HasField("_timeline")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---------- Calculated, not columns ----------
        builder.Ignore(s => s.TotalDeclaredValue);
        builder.Ignore(s => s.IsPaid);
        builder.Ignore(s => s.NeedsRefund);
        builder.Ignore(s => s.CanCancel);
        builder.Ignore(s => s.EstimatedArrival);

        // ---------- Lists ----------
        builder.HasIndex(s => new { s.SenderId, s.CreatedAt });                                 // "My shipments"
        builder.HasIndex(s => new { s.Status, s.FromCountry, s.ToCountry, s.CreatedAt });       // staff list + filters
        builder.HasIndex(s => s.PaymentReference);                                              // payment callbacks
    }
}