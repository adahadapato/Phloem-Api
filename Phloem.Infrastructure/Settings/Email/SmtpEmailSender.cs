using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Infrastructure.Settings.Email;

/// <summary>
/// Sends HTML emails through an SMTP server with MailKit (MIT licence). Implements the Domain's <see cref="IEmailSender"/>.
/// </summary>
/// <remarks>
/// <para>Every email comes FROM <see cref="EmailSettings.FromAddress"/> (no-reply) with a Reply-To header,
/// so answers reach a real team inbox.</para>
/// <para><b>Failures throw.</b> Callers decide whether an email failure matters (most log it and carry on).</para>
/// <para>Opens one connection per email. That's fine for Phloem's volume; a queue or bulk provider can
/// replace this class later without touching any caller.</para>
/// <para>Recipient addresses are not logged (personal data); only the subject is.</para>
/// </remarks>
internal sealed class SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger)
    : IEmailSender
{
    /// <inheritdoc />
    /// <exception cref="ArgumentException">Blank recipient, subject or body (a bug in the caller).</exception>
    /// <exception cref="ParseException">The recipient isn't a valid address.</exception>
    /// <exception cref="Exception">Any SMTP / network failure from MailKit.</exception>
    public Task SendAsync(string to, string subject, string htmlBody, EmailReplyTo replyTo,
                          CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        var settings = options.Value;

        return SendCoreAsync(to, subject, htmlBody, settings.ReplyTo.AddressFor(replyTo), cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>Replies to a team email go back to the team inbox itself.</remarks>
    /// <exception cref="ArgumentException">Blank subject or body (a bug in the caller).</exception>
    /// <exception cref="Exception">Any SMTP / network failure from MailKit.</exception>
    public Task SendToTeamAsync(string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        return SendCoreAsync(settings.TeamInbox, subject, htmlBody, settings.TeamInbox, cancellationToken);
    }

    /// <summary>Builds the message, connects, signs in if needed, sends and disconnects.</summary>
    /// <param name="to">Recipient.</param>
    /// <param name="subject">Subject.</param>
    /// <param name="htmlBody">HTML body.</param>
    /// <param name="replyToAddress">The Reply-To address.</param>
    /// <param name="cancellationToken">Cancels connecting / sending.</param>
    private async Task SendCoreAsync(string to, string subject, string htmlBody, string replyToAddress,
                                     CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlBody);

        var settings = options.Value;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.ReplyTo.Add(new MailboxAddress(settings.FromName, replyToAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(settings.Host, settings.Port, settings.Security, cancellationToken);

            if (!string.IsNullOrWhiteSpace(settings.Username))
                await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            logger.LogInformation("Email sent: {Subject}", subject);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Email failed: {Subject}", subject);
            throw;
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(quit: true, CancellationToken.None);
        }
    }
}