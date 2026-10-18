using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;

namespace Phloem.Application.Features.Accounts.Validators;

/// <summary>Both parts are required; the date itself is checked by the Domain (against today's date).</summary>
public sealed class UpdatePersonalDetailsCommandValidator : AbstractValidator<UpdatePersonalDetailsCommand>
{
    /// <summary>Sets up the rules.</summary>
    public UpdatePersonalDetailsCommandValidator()
    {
        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Please enter your date of birth.");
        RuleFor(x => x.HomeAddress).NotNull().SetValidator(new HomeAddressInputValidator());
    }
}