using Phloem.Domain.Enums;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Sends emails. Implemented in Infrastructure (SMTP, settings in <c>Email</c>).
/// </summary>
/// <remarks>
/// <para><b>Failures throw.</b> Callers that must not fail because of an email (e.g. a price-range change that is
/// already saved) catch the exception and log it.</para>
/// <para>Emails come FROM a no-reply address; the Reply-To header sends answers to the inbox chosen by
/// <see cref="EmailReplyTo"/>.</para>
/// <para>Bodies are HTML: callers must HTML-encode any user-entered text they put in them.</para>
/// </remarks>
public interface IEmailSender
{
    /// <summary>Sends an email to a customer or seller.</summary>
    /// <param name="to">The recipient's address.</param>
    /// <param name="subject">The subject line.</param>
    /// <param name="htmlBody">The body as HTML.</param>
    /// <param name="replyTo">Which team inbox gets the reply.</param>
    /// <param name="cancellationToken">Cancels sending.</param>
    Task SendAsync(string to, string subject, string htmlBody, EmailReplyTo replyTo,
                   CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email to Phloem's own team inbox (e.g. "New shop waiting for approval"), so handlers
    /// never hard-code staff addresses.
    /// </summary>
    /// <param name="subject">The subject line.</param>
    /// <param name="htmlBody">The body as HTML.</param>
    /// <param name="cancellationToken">Cancels sending.</param>
    Task SendToTeamAsync(string subject, string htmlBody, CancellationToken cancellationToken = default);
}