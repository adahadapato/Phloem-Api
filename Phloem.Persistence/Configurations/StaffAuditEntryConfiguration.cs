using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>StaffAuditEntries</c>: the staff audit log (only ever added to).</summary>
/// <remarks>
/// Indexes serve the owners' log page: newest first, by person, and by action.
/// </remarks>
public sealed class StaffAuditEntryConfiguration : EntityConfiguration<StaffAuditEntry>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<StaffAuditEntry> builder)
    {
        builder.ToTable("StaffAuditEntries");

        builder.Property(a => a.ActorId).IsRequired();
        builder.Property(a => a.Action).IsRequired();
        builder.Property(a => a.Target).IsRequired().HasMaxLength(StaffAuditEntry.TargetMaxLength);
        builder.Property(a => a.Country);
        builder.Property(a => a.Detail).HasMaxLength(StaffAuditEntry.DetailMaxLength);

        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.ActorId, a.CreatedAt });
        builder.HasIndex(a => new { a.Action, a.CreatedAt });
    }
}
