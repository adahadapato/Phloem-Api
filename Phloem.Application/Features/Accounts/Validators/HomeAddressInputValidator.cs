using FluentValidation;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Accounts.Validators;

/// <summary>Rules for a home address, shared by sign-up and the personal-details page.</summary>
public sealed class HomeAddressInputValidator : AbstractValidator<HomeAddressInput>
{
    /// <summary>Sets up the rules (sizes come from <see cref="ResidentialAddress"/>).</summary>
    public HomeAddressInputValidator()
    {
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(ResidentialAddress.Line1MaxLength);
        RuleFor(x => x.Line2).MaximumLength(ResidentialAddress.Line2MaxLength);
        RuleFor(x => x.City).NotEmpty().MaximumLength(ResidentialAddress.CityMaxLength);
        RuleFor(x => x.RegionOrPostcode).NotEmpty().MaximumLength(ResidentialAddress.RegionOrPostcodeMaxLength);
        RuleFor(x => x.Country).IsInEnum();
    }
}