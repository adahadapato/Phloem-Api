using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="UpdateShipmentProgressCommand"/>.</summary>
public sealed class UpdateShipmentProgressCommandValidator : AbstractValidator<UpdateShipmentProgressCommand>
{
    /// <summary>The four journey steps this command can move to.</summary>
    private static readonly ShipmentStatus[] JourneySteps =
    [
        ShipmentStatus.InTransit, ShipmentStatus.ArrivedInDestination,
        ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered
    ];

    /// <summary>Tracking number, a journey status only, and a short optional note.</summary>
    public UpdateShipmentProgressCommandValidator()
    {
        TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));

        RuleFor(x => x.NewStatus)
            .Must(s => JourneySteps.Contains(s))
            .WithMessage("Choose In transit, Arrived, Out for delivery or Delivered.");

        RuleFor(x => x.Note).MaximumLength(ShipmentEvent.NoteMaxLength);
    }
}
