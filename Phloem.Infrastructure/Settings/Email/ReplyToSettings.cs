using System.ComponentModel.DataAnnotations;
using Phloem.Application.Common;
using Phloem.Domain.Enums;

namespace Phloem.Infrastructure.Settings.Email;

/// <summary>
/// The <c>"Email:ReplyTo"</c> sub-section (keys in <see cref="AppSettings.Email.ReplyTo"/>): one inbox per
/// <see cref="EmailReplyTo"/>. All three may be the same address.
/// </summary>
public sealed class ReplyToSettings
{
    /// <summary>Customers' replies, e.g. "support@phloem.app".</summary>
    [Required, EmailAddress]
    public string Support { get; set; } = string.Empty;

    /// <summary>Sellers' replies, e.g. "sellers@phloem.app".</summary>
    [Required, EmailAddress]
    public string Sellers { get; set; } = string.Empty;

    /// <summary>Parcel senders' replies, e.g. "shipping@phloem.app".</summary>
    [Required, EmailAddress]
    public string Shipping { get; set; } = string.Empty;

    /// <summary>The address for one audience.</summary>
    /// <param name="replyTo">The audience.</param>
    /// <returns>The configured inbox.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value added to <see cref="EmailReplyTo"/> without a setting here (a bug).</exception>
    public string AddressFor(EmailReplyTo replyTo) => replyTo switch
    {
        EmailReplyTo.Support => Support,
        EmailReplyTo.Sellers => Sellers,
        EmailReplyTo.Shipping => Shipping,
        _ => throw new ArgumentOutOfRangeException(nameof(replyTo), replyTo, "No reply-to address is configured for this audience.")
    };
}