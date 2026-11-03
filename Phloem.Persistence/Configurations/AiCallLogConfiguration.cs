using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>AiCallLogs</c>: every AI call (spend cap, checking jobs, training data later).</summary>
/// <remarks>Indexes serve the month's spend (by time), the log page (by job and time), and the clean-up.</remarks>
public sealed class AiCallLogConfiguration : EntityConfiguration<AiCallLog>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<AiCallLog> builder)
    {
        builder.ToTable("AiCallLogs");

        builder.Property(c => c.PromptKey).IsRequired().HasMaxLength(AiPromptVersion.KeyMaxLength);
        builder.Property(c => c.PromptVersion).IsRequired();
        builder.Property(c => c.Subject).HasMaxLength(AiCallLog.SubjectMaxLength);
        builder.Property(c => c.Provider).IsRequired();
        builder.Property(c => c.Model).IsRequired().HasMaxLength(AiCallLog.ModelMaxLength);
        builder.Property(c => c.Input).IsRequired().HasMaxLength(AiCallLog.TextMaxLength);
        builder.Property(c => c.Output).HasMaxLength(AiCallLog.TextMaxLength);
        builder.Property(c => c.Outcome).IsRequired();
        builder.Property(c => c.Error).HasMaxLength(AiCallLog.ErrorMaxLength);
        builder.Property(c => c.InputTokens).IsRequired();
        builder.Property(c => c.OutputTokens).IsRequired();
        builder.Property(c => c.CostUsd).IsRequired().HasPrecision(12, 6);
        builder.Property(c => c.DurationMs).IsRequired();
        builder.Property(c => c.FinalDecision).HasMaxLength(AiCallLog.DecisionMaxLength);
        builder.Property(c => c.DecidedBy);
        builder.Property(c => c.DecidedAtUtc);

        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => new { c.PromptKey, c.CreatedAt });
    }
}
