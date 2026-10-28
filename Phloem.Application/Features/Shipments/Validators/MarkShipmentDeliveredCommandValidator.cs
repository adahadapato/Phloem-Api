using FluentValidation;
using Phloem.Application.Features.Deliveries.Validators;
using Phloem.Application.Features.Shipments.Commands;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="MarkShipmentDeliveredCommand"/>.</summary>
public sealed class MarkShipmentDeliveredCommandValidator : AbstractValidator<MarkShipmentDeliveredCommand>
{
    /// <summary>Tracking number, plus the delivery photo and handover rules.</summary>
    public MarkShipmentDeliveredCommandValidator()
    {
        TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
        DeliveryProofRules.Apply(this, x => x.Photo, x => x.PhotoSizeBytes, x => x.Handover, x => x.ReceivedBy,
                                 x => x.Note);
    }
}
