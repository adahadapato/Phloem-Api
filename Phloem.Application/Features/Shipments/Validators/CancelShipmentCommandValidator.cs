using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="CancelShipmentCommand"/>.</summary>
public sealed class CancelShipmentCommandValidator : AbstractValidator<CancelShipmentCommand>
{
    /// <summary>Tracking number required.</summary>
    public CancelShipmentCommandValidator() => TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
}
