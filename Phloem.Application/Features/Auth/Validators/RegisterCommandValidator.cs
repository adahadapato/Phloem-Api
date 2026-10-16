using FluentValidation;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Auth.Validators;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(UserProfile.FullNameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(EmailAddress.IsValid).WithMessage(EmailAddress.InvalidMessage);

        RuleFor(x => x.PhoneNumber)
            .Must(v => string.IsNullOrWhiteSpace(v) || PhoneNumber.IsValid(v))
            .WithMessage(PhoneNumber.InvalidMessage);

        RuleFor(x => x.Address).MaximumLength(DeliveryAddress.FullAddressMaxLength);
        RuleFor(x => x.Country).IsInEnum();

        // Strength (length, digits…) is Identity's job; its message comes back as a Result
        RuleFor(x => x.Password).NotEmpty();

        RuleFor(x => x.AgreedToTerms)
            .Equal(true).WithMessage("Please accept the Terms & Conditions.");
    }
}