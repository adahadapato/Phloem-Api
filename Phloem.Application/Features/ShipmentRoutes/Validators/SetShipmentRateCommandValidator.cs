using FluentValidation;
using Phloem.Application.Features.ShipmentRoutes.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.ShipmentRoutes.Validators;

/// <summary>Input checks for <see cref="SetShipmentRateCommand"/>.</summary>
public sealed class SetShipmentRateCommandValidator : AbstractValidator<SetShipmentRateCommand>
{
    /// <summary>Real values; amounts 0 or more; transit days 1–90 with max ≥ min.</summary>
    public SetShipmentRateCommandValidator()
    {
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum();
        RuleFor(x => x.Speed).IsInEnum();
        RuleFor(x => x.BaseCharge).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PricePerKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinTransitDays).InclusiveBetween(1, ShipmentRate.MaxTransitDaysLimit);
        RuleFor(x => x.MaxTransitDays)
            .InclusiveBetween(1, ShipmentRate.MaxTransitDaysLimit)
            .GreaterThanOrEqualTo(x => x.MinTransitDays)
            .WithMessage("The most days can't be fewer than the fewest days.");
    }
}
