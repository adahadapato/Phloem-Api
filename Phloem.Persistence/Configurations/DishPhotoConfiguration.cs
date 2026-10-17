using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Persistence.Mappings;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>DishPhotos</c>: one processed photo of a dish, with the file's details stored as columns
/// (<c>File_Path</c>, <c>File_Dimensions_Width</c>…). Child of <see cref="Dish"/>; the relationship is configured in
/// <see cref="DishConfiguration"/>.
/// </summary>
/// <remarks>
/// Removing a photo deletes this row but NEVER the image files: order lines keep pointing at them
/// (see the backlog's "Dish photo files are never deleted").
/// </remarks>
public sealed class DishPhotoConfiguration : EntityConfiguration<DishPhoto>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<DishPhoto> builder)
    {
        builder.ToTable("DishPhotos");

        builder.Property(p => p.DishId).IsRequired();
        builder.Property(p => p.AltText).IsRequired().HasMaxLength(DishPhoto.AltTextMaxLength);
        builder.Property(p => p.DisplayOrder).IsRequired();
        builder.Property(p => p.IsPrimary).IsRequired();

        builder.OwnsOne(p => p.File, file => file.MapImageFile());
        builder.Navigation(p => p.File).IsRequired();

        builder.HasIndex(p => new { p.DishId, p.DisplayOrder });   // photos in order
    }
}