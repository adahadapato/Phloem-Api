using FluentValidation;
using Phloem.Application.Features.ShipmentRoutes.Commands;

namespace Phloem.Application.Features.ShipmentRoutes.Validators;

/// <summary>Input checks for <see cref="RemoveShipmentRateCommand"/>.</summary>
public sealed class RemoveShipmentRateCommandValidator : AbstractValidator<RemoveShipmentRateCommand>
{
    /// <summary>Real values.</summary>
    public RemoveShipmentRateCommandValidator()
    {
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum();
        RuleFor(x => x.Speed).IsInEnum();
    }
}
