using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="SendOutShipmentCommand"/>.</summary>
public sealed class SendOutShipmentCommandValidator : AbstractValidator<SendOutShipmentCommand>
{
    /// <summary>Tracking number, a courier, and a short optional note.</summary>
    public SendOutShipmentCommandValidator()
    {
        TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
        RuleFor(x => x.CourierId).NotEmpty().WithMessage("Choose a courier.");
        RuleFor(x => x.Note).MaximumLength(ShipmentEvent.NoteMaxLength);
    }
}
