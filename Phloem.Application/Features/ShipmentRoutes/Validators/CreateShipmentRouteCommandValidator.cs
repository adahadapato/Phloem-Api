using FluentValidation;
using Phloem.Application.Features.ShipmentRoutes.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.ShipmentRoutes.Validators;

/// <summary>Input checks for <see cref="CreateShipmentRouteCommand"/>.</summary>
public sealed class CreateShipmentRouteCommandValidator : AbstractValidator<CreateShipmentRouteCommand>
{
    /// <summary>Two different real countries and the hub details.</summary>
    public CreateShipmentRouteCommandValidator()
    {
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum()
            .NotEqual(x => x.FromCountry).WithMessage("A route must go between two different countries.");
        RuleFor(x => x.DropOffAddress).NotEmpty().MaximumLength(ShipmentRoute.DropOffAddressMaxLength);
        RuleFor(x => x.DropOffHours).NotEmpty().MaximumLength(ShipmentRoute.DropOffHoursMaxLength);
    }
}
