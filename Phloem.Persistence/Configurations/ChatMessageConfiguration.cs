using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>ChatMessages</c>: every message of every order chat (never edited).</summary>
public sealed class ChatMessageConfiguration : EntityConfiguration<ChatMessage>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.Property(m => m.ChatId).IsRequired();
        builder.Property(m => m.SenderId).IsRequired();
        builder.Property(m => m.SenderRole).IsRequired();
        builder.Property(m => m.Body).IsRequired().HasMaxLength(ChatMessage.BodyMaxLength);
        builder.Property(m => m.PhotoPath).HasMaxLength(500);
        builder.Property(m => m.HiddenBySender).IsRequired();
        builder.Property(m => m.HasContactDetails).IsRequired();

        builder.HasOne<OrderChat>().WithMany().HasForeignKey(m => m.ChatId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => new { m.ChatId, m.CreatedAt });
    }
}
