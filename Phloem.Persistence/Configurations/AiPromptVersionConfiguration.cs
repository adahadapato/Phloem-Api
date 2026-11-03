using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>AiPromptVersions</c>: every saved wording of every AI prompt (only ever added to).</summary>
public sealed class AiPromptVersionConfiguration : EntityConfiguration<AiPromptVersion>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<AiPromptVersion> builder)
    {
        builder.ToTable("AiPromptVersions");

        builder.Property(v => v.Key).IsRequired().HasMaxLength(AiPromptVersion.KeyMaxLength);
        builder.Property(v => v.Version).IsRequired();
        builder.Property(v => v.Text).IsRequired().HasMaxLength(AiPromptVersion.TextMaxLength);
        builder.Property(v => v.Note).HasMaxLength(AiPromptVersion.NoteMaxLength);
        builder.Property(v => v.SavedBy).IsRequired();

        // One number per version of a key; also finds the newest quickly
        builder.HasIndex(v => new { v.Key, v.Version }).IsUnique();
    }
}
