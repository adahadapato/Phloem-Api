using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>ModerationChecks</c>: every check of people's text (dishes, shop names, dish names, messages).</summary>
/// <remarks>Indexes serve the background checker (by state), the queue (state, kind, country) and "the latest check
/// of these targets" (kind, target).</remarks>
public sealed class ModerationCheckConfiguration : EntityConfiguration<ModerationCheck>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ModerationCheck> builder)
    {
        builder.ToTable("ModerationChecks");

        builder.Property(c => c.Kind).IsRequired();
        builder.Property(c => c.TargetId).IsRequired();
        builder.Property(c => c.Country);
        builder.Property(c => c.Text).IsRequired().HasMaxLength(ModerationCheck.TextMaxLength);
        builder.Property(c => c.State).IsRequired();
        builder.Property(c => c.AiVerdict);
        builder.Property(c => c.AiReason).HasMaxLength(ModerationCheck.ReasonMaxLength);
        builder.Property(c => c.AiLabel).HasMaxLength(ModerationCheck.LabelMaxLength);
        builder.Property(c => c.IsSpam);
        builder.Property(c => c.IsUrgent);
        builder.Property(c => c.AiCallId);
        builder.Property(c => c.AiAttempts).IsRequired();
        builder.Property(c => c.StaffVerdict);
        builder.Property(c => c.StaffNote).HasMaxLength(ModerationCheck.ReasonMaxLength);
        builder.Property(c => c.DecidedBy);
        builder.Property(c => c.DecidedAtUtc);

        builder.HasIndex(c => new { c.State, c.CreatedAt });
        builder.HasIndex(c => new { c.State, c.Kind, c.Country });
        builder.HasIndex(c => new { c.Kind, c.TargetId, c.CreatedAt });
    }
}
