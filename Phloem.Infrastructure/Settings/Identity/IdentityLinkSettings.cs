using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// The <c>"Identity:Links"</c> sub-section (keys in <see cref="AppSettings.Identity.Links"/>): which website pages
/// the confirmation and password-reset emails link to.
/// </summary>
/// <remarks>Each path is joined to <c>AppSettings:WebsiteBaseUrl</c>; the query string (user, token) is added by
/// <see cref="AuthService"/>.</remarks>
public sealed class IdentityLinkSettings
{
    /// <summary>"Confirm your email" page, e.g. "/confirm-email".</summary>
    [Required]
    public string ConfirmEmailPath { get; set; } = string.Empty;

    /// <summary>"Choose a new password" page, e.g. "/reset-password".</summary>
    [Required]
    public string ResetPasswordPath { get; set; } = string.Empty;
}