using FluentValidation;
using Phloem.Application.Features.ShipmentRoutes.Commands;

namespace Phloem.Application.Features.ShipmentRoutes.Validators;

/// <summary>Input checks for <see cref="SetShipmentRouteActiveCommand"/>.</summary>
public sealed class SetShipmentRouteActiveCommandValidator : AbstractValidator<SetShipmentRouteActiveCommand>
{
    /// <summary>Real countries.</summary>
    public SetShipmentRouteActiveCommandValidator()
    {
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum();
    }
}
