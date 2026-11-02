using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>PendingChanges</c>: changes waiting for (or decided by) a second person.</summary>
/// <remarks>
/// <see cref="PendingChange.Payload"/> is nvarchar(max): it can hold a whole Terms document. Indexes serve the
/// Approvals page (waiting, by kind and record) and the expiry task.
/// </remarks>
public sealed class PendingChangeConfiguration : EntityConfiguration<PendingChange>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<PendingChange> builder)
    {
        builder.ToTable("PendingChanges");

        builder.Property(p => p.Kind).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.Country);
        builder.Property(p => p.TargetId);
        builder.Property(p => p.Summary).IsRequired().HasMaxLength(PendingChange.SummaryMaxLength);
        builder.Property(p => p.Payload).IsRequired();
        builder.Property(p => p.ProposedById).IsRequired();
        builder.Property(p => p.ExpiresAtUtc).IsRequired();
        builder.Property(p => p.DecidedById);
        builder.Property(p => p.DecidedAtUtc);
        builder.Property(p => p.DecisionNote).HasMaxLength(PendingChange.DecisionNoteMaxLength);
        builder.Property(p => p.Outcome).HasMaxLength(PendingChange.OutcomeMaxLength);

        builder.HasIndex(p => new { p.Status, p.ExpiresAtUtc });          // waiting list + expiry task
        builder.HasIndex(p => new { p.Kind, p.TargetId, p.Status });      // "is this shop's block waiting?"
        builder.HasIndex(p => new { p.ProposedById, p.CreatedAt });       // "my proposals"
        builder.HasIndex(p => p.CreatedAt);                               // history, newest first
    }
}
