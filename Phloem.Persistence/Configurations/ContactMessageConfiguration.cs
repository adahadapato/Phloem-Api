using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>ContactMessages</c>: the Contact us inbox (Admin → Contact messages).</summary>
/// <remarks>Read newest first, often by status, hence the index. <c>UserId</c> isn't a foreign key: a message
/// stays in the inbox even if the account is later deleted.</remarks>
public sealed class ContactMessageConfiguration : EntityConfiguration<ContactMessage>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");

        builder.Property(m => m.Name).IsRequired().HasMaxLength(ContactMessage.NameMaxLength);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(ContactMessage.EmailMaxLength);
        builder.Property(m => m.Topic).IsRequired();
        builder.Property(m => m.Reference).HasMaxLength(ContactMessage.ReferenceMaxLength);
        builder.Property(m => m.Message).IsRequired().HasMaxLength(ContactMessage.MessageMaxLength);
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.StaffNote).HasMaxLength(ContactMessage.NoteMaxLength);

        builder.HasIndex(m => new { m.Status, m.CreatedAt });
    }
}
