using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Persistence.Mappings;

/// <summary>
/// Maps the courier columns and the <see cref="ProofOfDelivery"/> (as <c>DeliveryProof_PhotoPath</c>…) inside an order
/// or shipment table. Used by OrderConfiguration and ShipmentConfiguration, which have the same delivery columns.
/// </summary>
/// <remarks>
/// <para>The proof is optional (null until delivered), so EF Core makes its columns nullable and reads "all null" back
/// as no proof.</para>
/// <para>The courier link is a foreign key to <c>UserProfiles</c> with Restrict (profiles are soft-deleted, never
/// removed). No navigation property: names come from the <c>CourierName</c> snapshot.</para>
/// </remarks>
public static class ProofOfDeliveryMapping
{
    /// <summary>Maps the proof's columns.</summary>
    /// <typeparam name="TOwner">The order or shipment.</typeparam>
    /// <param name="proof">EF Core's builder for the owned proof.</param>
    public static void MapProofOfDelivery<TOwner>(this OwnedNavigationBuilder<TOwner, ProofOfDelivery> proof)
        where TOwner : class
    {
        proof.Property(p => p.PhotoPath).IsRequired().HasMaxLength(ProofOfDelivery.PhotoPathMaxLength);
        proof.Property(p => p.Handover).IsRequired();
        proof.Property(p => p.ReceivedBy).HasMaxLength(ProofOfDelivery.ReceivedByMaxLength);
        proof.Property(p => p.Note).HasMaxLength(ProofOfDelivery.NoteMaxLength);
        proof.Property(p => p.CourierId).IsRequired();
        proof.Property(p => p.CapturedAtUtc).IsRequired();
    }

    /// <summary>Courier name length (a person's full name).</summary>
    public const int CourierNameMaxLength = UserProfile.FullNameMaxLength;

    /// <summary>The "who is delivering it" foreign key, with Restrict.</summary>
    /// <typeparam name="TOwner">The order or shipment.</typeparam>
    /// <param name="builder">The owner's builder.</param>
    /// <param name="courierId">The CourierId property.</param>
    public static void HasCourier<TOwner>(this EntityTypeBuilder<TOwner> builder,
                                          System.Linq.Expressions.Expression<Func<TOwner, object?>> courierId)
        where TOwner : class =>
        builder.HasOne<UserProfile>()
               .WithMany()
               .HasForeignKey(courierId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
}
