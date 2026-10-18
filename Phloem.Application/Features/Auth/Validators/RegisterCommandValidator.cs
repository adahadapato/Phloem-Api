using FluentValidation;
using Phloem.Application.Features.Accounts.Validators;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Auth.Validators;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(UserProfile.FirstNameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(UserProfile.LastNameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(EmailAddress.IsValid).WithMessage(EmailAddress.InvalidMessage);

        RuleFor(x => x.PhoneNumber)
            .Must(v => string.IsNullOrWhiteSpace(v) || PhoneNumber.IsValid(v))
            .WithMessage(PhoneNumber.InvalidMessage);

        RuleFor(x => x.Country).IsInEnum();

        // Optional, but checked when given
        RuleFor(x => x.HomeAddress!).SetValidator(new HomeAddressInputValidator()).When(x => x.HomeAddress is not null);

        // Strength (length, digits…) is Identity's job; its message comes back as a Result
        RuleFor(x => x.Password).NotEmpty();

        RuleFor(x => x.AgreedToTerms)
            .Equal(true).WithMessage("Please accept the Terms & Conditions.");
    }
}