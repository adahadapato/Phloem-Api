using FluentValidation;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Auth.Validators;

/// <summary>
/// Checks <see cref="SignInWithExternalLoginCommand"/> before the handler runs. The values come from Google, but are
/// still checked: the handler relies on a valid email and a known country.
/// </summary>
public sealed class SignInWithExternalLoginCommandValidator : AbstractValidator<SignInWithExternalLoginCommand>
{
    /// <summary>Longest provider name / key accepted (ASP.NET Identity's login table allows 128 characters).</summary>
    private const int LoginFieldMaxLength = 128;

    /// <summary>Sets up the rules.</summary>
    public SignInWithExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(LoginFieldMaxLength);
        RuleFor(x => x.ProviderKey).NotEmpty().MaximumLength(LoginFieldMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(EmailAddress.IsValid).WithMessage(EmailAddress.InvalidMessage);

        RuleFor(x => x.Country).IsInEnum();
    }
}
