using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>
/// "Continue with Google", after the person picked their account on Google's page. Finds, links or creates the
/// Phloem account and returns a ONE-TIME code for the app (no tokens yet).
/// </summary>
/// <remarks>
/// <para>Sent by the API's Google callback, never by the app directly: the values come from Google's verified answer,
/// already turned into plain values by Presentation (Application knows nothing about Google or HTTP).</para>
/// <para>The app swaps the code for tokens with <see cref="ExchangeLoginCodeCommand"/>, where suspension and 2FA are
/// checked.</para>
/// </remarks>
/// <param name="Provider">The provider's name, e.g. "Google".</param>
/// <param name="ProviderKey">The provider's permanent Id for the person.</param>
/// <param name="Email">The email the provider gave.</param>
/// <param name="EmailVerified">True when the provider confirmed the person owns the email.</param>
/// <param name="FirstName">First name, if the provider shared it.</param>
/// <param name="LastName">Last name, if the provider shared it.</param>
/// <param name="Country">The country picked in the app's flag selector; used ONLY when a new account is created.</param>
public sealed record SignInWithExternalLoginCommand(
    string Provider,
    string ProviderKey,
    string Email,
    bool EmailVerified,
    string? FirstName,
    string? LastName,
    Country Country) : IRequest<Result<string>>;
