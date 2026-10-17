using System.ComponentModel.DataAnnotations;
using MailKit.Security;
using Phloem.Application.Common;

namespace Phloem.Infrastructure.Settings.Email;

/// <summary>
/// The <c>"Email"</c> section of appsettings.json: who emails come from, where replies go, the team inbox,
/// and which SMTP server sends them. Keys are listed in <see cref="AppSettings.Email"/>.
/// </summary>
/// <remarks>
/// <para>Checked when the app starts (see <see cref="EmailSetup"/>), so a missing value stops the app immediately
/// instead of failing on the first email.</para>
/// <para><b>Never put the password in appsettings.json.</b> Use User Secrets in development
/// (<c>dotnet user-secrets set "Email:Password" "..."</c>) and environment variables or a key vault in
/// production (<c>Email__Password</c>).</para>
/// <para>Development tip: run a local test inbox such as smtp4dev (Host "localhost", Port 25,
/// Security "None", no username). Every email the API sends appears there, and nothing reaches real people.</para>
/// </remarks>
public sealed class EmailSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Email.Section;

    /// <summary>The name people see, e.g. "Phloem".</summary>
    [Required]
    public string FromName { get; set; } = string.Empty;

    /// <summary>The sending address, e.g. "no-reply@phloem.app". Replies go to <see cref="ReplyTo"/> instead.</summary>
    [Required, EmailAddress]
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>Phloem staff's own inbox, for emails TO the team (e.g. "New shop waiting for approval").</summary>
    [Required, EmailAddress]
    public string TeamInbox { get; set; } = string.Empty;

    /// <summary>Where replies go, one inbox per audience.</summary>
    [Required]
    public ReplyToSettings ReplyTo { get; set; } = new();

    /// <summary>SMTP server, e.g. "smtp.zoho.eu" or "localhost".</summary>
    [Required]
    public string Host { get; set; } = string.Empty;

    /// <summary>SMTP port: usually 587 (StartTls), 465 (SslOnConnect) or 25 (local test inbox).</summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    /// <summary>Encryption: "StartTls" (default), "SslOnConnect", "Auto" or "None" (local testing only).</summary>
    public SecureSocketOptions Security { get; set; } = SecureSocketOptions.StartTls;

    /// <summary>SMTP user name; leave empty when the server needs no sign-in (local test inbox).</summary>
    public string? Username { get; set; }

    /// <summary>SMTP password. From User Secrets / environment variables only.</summary>
    public string? Password { get; set; }
}