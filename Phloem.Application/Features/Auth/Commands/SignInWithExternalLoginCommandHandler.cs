using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>
/// Handles <see cref="SignInWithExternalLoginCommand"/>: existing account (found or linked) → one-time code; no account
/// yet → creates the login account AND its UserProfile in ONE transaction (like <see cref="RegisterCommandHandler"/>),
/// then the code.
/// </summary>
/// <remarks>
/// <para><b>Names:</b> the provider may leave a name out, and a profile can't have a blank one, so a missing name
/// becomes the part of the email before the "@". The person can change it on their profile.</para>
/// <para><b>No phone, no address:</b> both stay optional, exactly as on the Register page; they can be added
/// later.</para>
/// </remarks>
/// <param name="auth">Finds, links and creates login accounts.</param>
/// <param name="profiles">Adds the new UserProfile.</param>
/// <param name="codes">Issues the one-time code.</param>
/// <param name="unitOfWork">Saves the account and profile together.</param>
public sealed class SignInWithExternalLoginCommandHandler(
    IAuthService auth,
    IUserProfileRepository profiles,
    ILoginCodeStore codes,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SignInWithExternalLoginCommand, Result<string>>
{
    /// <summary>Finds or creates the account and returns the one-time code.</summary>
    /// <param name="command">Who the provider says the person is, and the app's country.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>The one-time code; or <c>AuthErrors.ExternalEmailNotVerified</c>, <c>AuthErrors.EmailTaken</c> or an
    /// Identity error.</returns>
    public async Task<Result<string>> Handle(SignInWithExternalLoginCommand command, CancellationToken ct)
    {
        var email = EmailAddress.Create(command.Email).Value;   // trimmed + lower-case (validator checked it)
        var identity = new ExternalIdentity(command.Provider.Trim(), command.ProviderKey.Trim(), email,
                                            command.EmailVerified, command.FirstName, command.LastName);

        // 1. Existing account (already linked, or linked now by its email)
        var found = await auth.FindExternalLoginAsync(identity, ct);
        if (found.IsSuccess) return codes.Issue(found.Value);
        if (!found.Error.Equals(AuthErrors.NoAccountForExternalLogin)) return found.Error;

        // 2. New person: login account + profile with the SAME Id, in one transaction
        var created = await CreateAccountAsync(identity, command, ct);
        if (created.IsFailure) return created.Error;

        return codes.Issue(created.Value);
    }

    /// <summary>Creates the login account and its profile; nothing is kept if either fails.</summary>
    /// <param name="identity">Who the provider says the person is.</param>
    /// <param name="command">The command (names, country).</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>The new account's Id; or the error that stopped it (no commit = rollback).</returns>
    private async Task<Result<Guid>> CreateAccountAsync(ExternalIdentity identity,
                                                         SignInWithExternalLoginCommand command,
                                                         CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        var registered = await auth.RegisterExternalAsync(identity, ct);
        if (registered.IsFailure) return registered.Error;

        var fallback = identity.Email.Split('@')[0];
        var profile = UserProfile.Create(registered.Value,
                                         NameOrFallback(command.FirstName, fallback, UserProfile.FirstNameMaxLength),
                                         NameOrFallback(command.LastName, fallback, UserProfile.LastNameMaxLength),
                                         command.Country);

        profiles.Add(profile);
        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return registered.Value;
    }

    /// <summary>The provider's name, or the fallback when it's missing; cut to the profile's limit.</summary>
    /// <param name="name">The name from the provider (may be empty).</param>
    /// <param name="fallback">Used when <paramref name="name"/> is empty.</param>
    /// <param name="maxLength">The profile's limit for this name.</param>
    /// <returns>A non-blank name no longer than <paramref name="maxLength"/>.</returns>
    private static string NameOrFallback(string? name, string fallback, int maxLength)
    {
        var value = string.IsNullOrWhiteSpace(name) ? fallback : name.Trim();
        return value.Length <= maxLength ? value : value[..maxLength].TrimEnd();
    }
}
