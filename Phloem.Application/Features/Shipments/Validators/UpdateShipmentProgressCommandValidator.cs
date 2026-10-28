using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="UpdateShipmentProgressCommand"/>.</summary>
public sealed class UpdateShipmentProgressCommandValidator : AbstractValidator<UpdateShipmentProgressCommand>
{
    /// <summary>
    /// The journey steps this command can move to. "Out for delivery" needs a courier (SendOutShipmentCommand) and
    /// "Delivered" needs the courier's photo (MarkShipmentDeliveredCommand), so neither is here.
    /// </summary>
    private static readonly ShipmentStatus[] JourneySteps =
    [
        ShipmentStatus.InTransit, ShipmentStatus.ArrivedInDestination
    ];

    /// <summary>Tracking number, a journey status only, and a short optional note.</summary>
    public UpdateShipmentProgressCommandValidator()
    {
        TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));

        RuleFor(x => x.NewStatus)
            .Must(s => JourneySteps.Contains(s))
            .WithMessage("Choose In transit or Arrived. For Out for delivery, choose a courier; " +
                         "the courier marks it Delivered with a photo.");

        RuleFor(x => x.Note).MaximumLength(ShipmentEvent.NoteMaxLength);
    }
}
