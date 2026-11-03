using Phloem.Application.Features.Chat.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Chat.Common;

/// <summary>Names and photo links for chat messages.</summary>
public sealed class ChatFormatting(IUserProfileRepository profiles, IFileStorage storage)
{
    /// <summary>How long a photo link works.</summary>
    public static readonly TimeSpan PhotoLinkLifetime = TimeSpan.FromMinutes(30);

    /// <summary>The line at the top of every chat.</summary>
    public const string Notice =
        "Messages are saved and may be reviewed by Phloem if there's a problem with this order. Please keep ordering " +
        "and paying on Phloem.";

    /// <summary>Full names of the customer and the shop's owner.</summary>
    public async Task<(string Customer, string Owner)> FullNamesAsync(Guid customerId, Guid ownerId, CancellationToken ct)
    {
        var names = await profiles.GetFullNamesAsync([customerId, ownerId], ct);
        return (names.GetValueOrDefault(customerId, "Customer"), names.GetValueOrDefault(ownerId, "Shop"));
    }

    /// <summary>"Grace (customer)" or "Ama (Mama's Kitchen)": first name and side, as customers and shops see each other.</summary>
    public static string PublicName(string fullName, ChatRole role, string shopName)
    {
        var trimmed = fullName.Trim();
        var space = trimmed.IndexOf(' ');
        var first = space > 0 ? trimmed[..space] : trimmed;
        return role == ChatRole.Customer ? $"{first} (customer)" : $"{first} ({shopName})";
    }

    /// <summary>A message as the customer or shop sees it.</summary>
    public async Task<ChatMessageDto> ForParticipantAsync(ChatMessage m, string customerName, string ownerName,
                                                          string shopName, CancellationToken ct) =>
        new(m.Id, m.SenderId, m.SenderRole,
            PublicName(m.SenderRole == ChatRole.Customer ? customerName : ownerName, m.SenderRole, shopName),
            m.Body, await PhotoLinkAsync(m, ct), m.CreatedAt, HiddenBySender: false, HasContactDetails: false);

    /// <summary>A message as staff see it: full names, deleted ones and contact-detail marks included.</summary>
    public async Task<ChatMessageDto> ForStaffAsync(ChatMessage m, string customerName, string ownerName,
                                                    CancellationToken ct) =>
        new(m.Id, m.SenderId, m.SenderRole,
            m.SenderRole == ChatRole.Customer ? $"{customerName} · Customer" : $"{ownerName} · Shop",
            m.Body, await PhotoLinkAsync(m, ct), m.CreatedAt, m.HiddenBySender, m.HasContactDetails);

    /// <summary>"📷 Photo" or the text, cut for lists.</summary>
    public static string Preview(ChatMessage m)
    {
        var text = m.Body.Length == 0 ? "📷 Photo" : m.Body;
        return text.Length <= 80 ? text : text[..80] + "…";
    }

    private async Task<string?> PhotoLinkAsync(ChatMessage m, CancellationToken ct) =>
        m.PhotoPath is null ? null : (await storage.GetTemporaryUrlAsync(m.PhotoPath, PhotoLinkLifetime, ct)).ToString();
}
