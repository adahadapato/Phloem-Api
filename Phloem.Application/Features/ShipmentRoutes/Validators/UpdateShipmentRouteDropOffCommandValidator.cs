using FluentValidation;
using Phloem.Application.Features.ShipmentRoutes.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.ShipmentRoutes.Validators;

/// <summary>Input checks for <see cref="UpdateShipmentRouteDropOffCommand"/>.</summary>
public sealed class UpdateShipmentRouteDropOffCommandValidator : AbstractValidator<UpdateShipmentRouteDropOffCommand>
{
    /// <summary>Real countries and the hub details.</summary>
    public UpdateShipmentRouteDropOffCommandValidator()
    {
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum();
        RuleFor(x => x.DropOffAddress).NotEmpty().MaximumLength(ShipmentRoute.DropOffAddressMaxLength);
        RuleFor(x => x.DropOffHours).NotEmpty().MaximumLength(ShipmentRoute.DropOffHoursMaxLength);
    }
}
