using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="MarkShipmentPaidCommand"/>.</summary>
public sealed class MarkShipmentPaidCommandValidator : AbstractValidator<MarkShipmentPaidCommand>
{
    /// <summary>Tracking number and payment reference required.</summary>
    public MarkShipmentPaidCommandValidator()
    {
        TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
        RuleFor(x => x.PaymentReference).NotEmpty().MaximumLength(Shipment.PaymentReferenceMaxLength);
    }
}
