namespace Phloem.Domain.Models;

/// <summary>
/// Who an outside provider (Google today) says the person is, after they signed in on the provider's own page.
/// An INPUT for the external sign-in methods of <c>IAuthService</c>; never stored as it is.
/// </summary>
/// <remarks>
/// <para><see cref="Provider"/> + <see cref="ProviderKey"/> identify the person for good: the email can change at the
/// provider, the key never does. They are saved in ASP.NET Identity's login table when the login is linked.</para>
/// <para>An existing Phloem account is linked by email only when <see cref="EmailVerified"/> is true, because only
/// then has the provider proved the person owns that email.</para>
/// </remarks>
/// <param name="Provider">The provider's name, e.g. "Google".</param>
/// <param name="ProviderKey">The provider's permanent Id for this person (Google's "sub").</param>
/// <param name="Email">The email the provider gave.</param>
/// <param name="EmailVerified">True when the provider has confirmed the person owns <paramref name="Email"/>.</param>
/// <param name="FirstName">First (given) name, if the provider shared it.</param>
/// <param name="LastName">Last (family) name, if the provider shared it.</param>
public sealed record ExternalIdentity(
    string Provider,
    string ProviderKey,
    string Email,
    bool EmailVerified,
    string? FirstName,
    string? LastName);
