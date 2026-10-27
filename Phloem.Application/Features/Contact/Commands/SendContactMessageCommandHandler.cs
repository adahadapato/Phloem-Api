using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Contact.Commands;

/// <summary>Handles <see cref="SendContactMessageCommand"/>.</summary>
public sealed class SendContactMessageCommandHandler(
    IContactMessageRepository messages,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IEmailSender email,
    IContactDetailsProvider contact,
    ILogger<SendContactMessageCommandHandler> logger)
    : IRequestHandler<SendContactMessageCommand, Result>
{
    /// <summary>
    /// Saves first (so nothing is lost), then sends the two emails. An email that fails is logged, not reported:
    /// the message is safely in the inbox either way.
    /// </summary>
    public async Task<Result> Handle(SendContactMessageCommand command, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(command.Website))
        {
            logger.LogInformation("Contact us: dropped a message that filled the spam trap.");
            return Result.Success();
        }

        var message = ContactMessage.Create(command.Name, command.Email, command.Topic, command.Reference,
                                            command.Message, currentUser.UserId);
        messages.Add(message);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Contact us: new {Topic} message {Id}.", message.Topic, message.Id);

        await TellTheTeamAsync(message, ct);
        await ConfirmToSenderAsync(message, ct);
        return Result.Success();
    }

    private async Task TellTheTeamAsync(ContactMessage m, CancellationToken ct)
    {
        try
        {
            var reference = m.Reference is null ? "" : $"<p><b>Order / tracking number:</b> {Enc(m.Reference)}</p>";
            await email.SendToTeamAsync(
                $"[Contact us] {TopicText(m.Topic)}: {m.Name}",
                $"<p><b>{Enc(m.Name)}</b> ({Enc(m.Email)}){(m.UserId is null ? "" : ", signed in")} wrote about " +
                $"<b>{TopicText(m.Topic).ToLowerInvariant()}</b>:</p>" +
                reference +
                $"<blockquote style=\"white-space:pre-wrap\">{Enc(m.Message)}</blockquote>" +
                "<p>Press Reply to answer them. Then mark it Replied or Closed in Admin → Contact messages.</p>",
                m.Email, m.Name, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Contact us: couldn't email the team about message {Id}.", m.Id);
        }
    }

    private async Task ConfirmToSenderAsync(ContactMessage m, CancellationToken ct)
    {
        try
        {
            var replyTime = contact.Get().ReplyTime;
            await email.SendAsync(m.Email, "We've got your message",
                $"<p>Hi {Enc(m.Name)},</p>" +
                "<p>Thanks for getting in touch with Phloem. We've received your message" +
                (replyTime is null ? "" : $" and we'll reply {Enc(replyTime)}") + ".</p>" +
                $"<p>Here's what you sent:</p><blockquote style=\"white-space:pre-wrap\">{Enc(m.Message)}</blockquote>" +
                "<p>If you need to add anything, just reply to this email.</p>" +
                "<p>The Phloem team</p>",
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Contact us: couldn't send the confirmation for message {Id}.", m.Id);
        }
    }

    /// <summary>"An order", "A parcel"… for the subject line.</summary>
    private static string TopicText(ContactTopic topic) => topic switch
    {
        ContactTopic.Order => "An order",
        ContactTopic.Parcel => "A parcel",
        ContactTopic.Selling => "Selling on Phloem",
        ContactTopic.Account => "Their account",
        _ => "Something else"
    };

    private static string Enc(string text) => WebUtility.HtmlEncode(text);
}
