using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Contact.Commands;

/// <summary>
/// Contact us form (website or app), signed in or not: saves the message in the team's inbox, emails the team
/// (pressing Reply answers the sender) and sends the sender a "we've got your message" email.
/// </summary>
/// <param name="Name">Who's writing.</param>
/// <param name="Email">Where to answer.</param>
/// <param name="Topic">Order, Parcel, Selling, Account or Other.</param>
/// <param name="Reference">Optional order or tracking number.</param>
/// <param name="Message">10–2,000 characters.</param>
/// <param name="Website">
/// Spam trap: a field people never see (hidden on the page). Robots fill every field, so when this has text the
/// message is quietly dropped, while the robot still sees "sent".
/// </param>
public sealed record SendContactMessageCommand(
    string Name,
    string Email,
    ContactTopic Topic,
    string? Reference,
    string Message,
    string? Website = null) : IRequest<Result>;
