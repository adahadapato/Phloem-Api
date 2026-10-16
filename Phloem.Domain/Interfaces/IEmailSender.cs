namespace Phloem.Domain.Interfaces;

/// <summary>
/// Sends an email (Infrastructure: SMTP / provider from AppSettings.Email).
/// Throws if sending fails; callers that must not fail because of email (e.g. the price-range
/// change) catch it and log it.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}