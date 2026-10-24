using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// The part of the <c>"Authentication"</c> section that Infrastructure reads: how long the one-time sign-in code
/// works. Keys are listed in <see cref="AppSettings.Authentication"/>.
/// </summary>
/// <remarks>Checked when the app starts (see <see cref="IdentitySetup"/>). The Google keys in the same section are read
/// by Presentation, which registers the Google sign-in itself.</remarks>
public sealed class LoginCodeSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Authentication.Section;

    /// <summary>Shortest allowed lifetime, in seconds: the app needs a moment to swap the code.</summary>
    public const int MinimumSeconds = 10;

    /// <summary>Longest allowed lifetime, in seconds: a code is meant to be used straight away.</summary>
    public const int MaximumSeconds = 600;

    /// <summary>How long a one-time code works, in seconds. Defaults to 60 when the key is missing.</summary>
    [Range(MinimumSeconds, MaximumSeconds,
           ErrorMessage = "Authentication:LoginCodeSeconds must be between 10 and 600.")]
    public int LoginCodeSeconds { get; set; } = 60;
}
