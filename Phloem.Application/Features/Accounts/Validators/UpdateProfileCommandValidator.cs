using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Accounts.Validators;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(UserProfile.FullNameMaxLength);
        RuleFor(x => x.Country).IsInEnum();

        RuleFor(x => x.PhoneNumber)
            .Must(v => string.IsNullOrWhiteSpace(v) || PhoneNumber.IsValid(v))
            .WithMessage(PhoneNumber.InvalidMessage);
    }
}