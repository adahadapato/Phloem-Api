using FluentValidation;
using Phloem.Application.Features.Staff.Commands;

namespace Phloem.Application.Features.Staff.Validators;

/// <summary>Checks <see cref="SetStaffCountriesCommand"/>: a person, and either "all" or at least one real country.</summary>
public sealed class SetStaffCountriesCommandValidator : AbstractValidator<SetStaffCountriesCommand>
{
    /// <summary>The rules.</summary>
    public SetStaffCountriesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Countries)
            .NotEmpty().WithMessage("Choose at least one country, or all countries.")
            .When(x => !x.AllCountries);

        RuleForEach(x => x.Countries).IsInEnum().WithMessage("That country isn't one Phloem works in.");
    }
}
