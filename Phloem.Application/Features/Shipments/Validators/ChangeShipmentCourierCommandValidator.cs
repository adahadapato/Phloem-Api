using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="ChangeShipmentCourierCommand"/>.</summary>
public sealed class ChangeShipmentCourierCommandValidator : AbstractValidator<ChangeShipmentCourierCommand>
{
    /// <summary>Tracking number and a courier.</summary>
    public ChangeShipmentCourierCommandValidator()
    {
        TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
        RuleFor(x => x.CourierId).NotEmpty().WithMessage("Choose a courier.");
    }
}
