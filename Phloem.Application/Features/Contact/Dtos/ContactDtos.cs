using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Contact.Dtos;

/// <summary><c>GET /api/contact</c>: what the Contact us page shows. Null values are left off the page.</summary>
/// <param name="Email">The support inbox.</param>
/// <param name="Phone">Phone number, or null.</param>
/// <param name="WhatsApp">WhatsApp number in international format, or null.</param>
/// <param name="Address">Postal address on one line, or null.</param>
/// <param name="OpeningHours">e.g. "Monday to Saturday, 9am to 6pm (UK time)", or null.</param>
/// <param name="ReplyTime">e.g. "within 24 hours", or null.</param>
public sealed record ContactDetailsDto(
    string Email,
    string? Phone,
    string? WhatsApp,
    string? Address,
    string? OpeningHours,
    string? ReplyTime)
{
    public static ContactDetailsDto From(ContactDetails d) =>
        new(d.Email, d.Phone, d.WhatsApp, d.Address, d.OpeningHours, d.ReplyTime);
}

/// <summary>One message in Admin → Contact messages.</summary>
/// <param name="Id">Used to change its status.</param>
/// <param name="Name">Who wrote.</param>
/// <param name="Email">Where to answer.</param>
/// <param name="Topic">Order, Parcel, Selling, Account or Other.</param>
/// <param name="Reference">Order or tracking number they gave, or null.</param>
/// <param name="Message">The message (plain text: show it as text, never as HTML).</param>
/// <param name="Status">New, Replied or Closed.</param>
/// <param name="StaffNote">A note for colleagues, or null.</param>
/// <param name="FromSignedInUser">True when they were signed in (the email is their account's).</param>
/// <param name="SentAt">When it was sent (UTC).</param>
/// <param name="HandledAt">When a team member last changed the status (UTC), or null.</param>
public sealed record ContactMessageDto(
    Guid Id,
    string Name,
    string Email,
    ContactTopic Topic,
    string? Reference,
    string Message,
    ContactMessageStatus Status,
    string? StaffNote,
    bool FromSignedInUser,
    DateTime SentAt,
    DateTime? HandledAt)
{
    public static ContactMessageDto From(ContactMessage m) =>
        new(m.Id, m.Name, m.Email, m.Topic, m.Reference, m.Message, m.Status, m.StaffNote, m.UserId is not null,
            m.CreatedAt, m.HandledAtUtc);
}

/// <summary><c>GET /api/admin/contact-messages</c>: one page of the inbox, plus how many there are of each status.</summary>
/// <param name="Messages">The page asked for (newest first).</param>
/// <param name="NewCount">Not looked at yet (the badge).</param>
/// <param name="RepliedCount">Replied, may be waiting for an answer back.</param>
/// <param name="ClosedCount">Done.</param>
public sealed record ContactInboxDto(
    PagedResult<ContactMessageDto> Messages,
    int NewCount,
    int RepliedCount,
    int ClosedCount);
