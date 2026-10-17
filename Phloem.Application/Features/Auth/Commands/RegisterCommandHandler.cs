using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>
/// Creates the login account AND the UserProfile (same Id) in ONE transaction,
/// then signs the new user in. We already know the name and email, so no extra lookups.
/// </summary>
public sealed class RegisterCommandHandler(
    IAuthService auth,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, Result<AuthTokensDto>>
{
    private const string SignUpAddressLabel = "Home";

    public async Task<Result<AuthTokensDto>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var email = EmailAddress.Create(command.Email).Value;    // trimmed + lower-case
        var phone = PhoneNumber.CreateOptional(command.PhoneNumber)?.Value;
        UserProfile profile;

        await using (var transaction = await unitOfWork.BeginTransactionAsync(ct))
        {
            // 1. Login account
            var registered = await auth.RegisterAsync(email, command.Password, phone, ct);
            if (registered.IsFailure) return registered.Error;   // no commit = rollback

            // 2. Profile with the SAME Id
            profile = UserProfile.Create(registered.Value, command.FirstName, command.LastName, command.Country);

            if (!string.IsNullOrWhiteSpace(command.Address))
            {
                var added = profile.AddAddress(SignUpAddressLabel, command.Address, command.Country, makeDefault: true);
                if (added.IsFailure) return added.Error;
            }

            profiles.Add(profile);
            await unitOfWork.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }

        // 3. Sign in (only after the account is definitely saved)
        var login = await auth.LoginAsync(email, command.Password, ct);
        if (login.IsFailure) return login.Error;

        var tokens = login.Value.Tokens
            ?? throw new InvalidOperationException("A new account should never require two-factor sign-in.");

        return AuthTokensDto.SignedIn(tokens, profile.GetFullName(), email);
    }
}