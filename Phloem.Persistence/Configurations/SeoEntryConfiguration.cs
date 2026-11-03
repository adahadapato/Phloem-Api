using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>SeoEntries</c>: search engine text, one row per website page.</summary>
public sealed class SeoEntryConfiguration : EntityConfiguration<SeoEntry>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<SeoEntry> builder)
    {
        builder.ToTable("SeoEntries");

        builder.Property(e => e.Key).IsRequired().HasMaxLength(SeoEntry.KeyMaxLength);
        builder.Property(e => e.Kind).IsRequired();
        builder.Property(e => e.ShopId);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(SeoPages.TitleMaxLength);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(SeoPages.DescriptionMaxLength);
        builder.Property(e => e.Keywords).IsRequired().HasMaxLength(SeoEntry.KeywordsMaxLength);
        builder.Property(e => e.Improvements).IsRequired().HasMaxLength(SeoEntry.ImprovementsMaxLength);
        builder.Property(e => e.Score);
        builder.Property(e => e.ScoreExplanation).HasMaxLength(SeoEntry.ScoreExplanationMaxLength);
        builder.Property(e => e.Source).IsRequired();
        builder.Property(e => e.SourceFingerprint).HasMaxLength(SeoEntry.FingerprintMaxLength);
        builder.Property(e => e.AiCallId);
        builder.Property(e => e.ChangedBy);

        builder.HasIndex(e => e.Key).IsUnique();
    }
}
