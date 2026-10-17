using FluentValidation;
using Phloem.Application.Features.Shipments.Queries;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="TrackShipmentQuery"/>.</summary>
public sealed class TrackShipmentQueryValidator : AbstractValidator<TrackShipmentQuery>
{
    /// <summary>Tracking number required.</summary>
    public TrackShipmentQueryValidator() => TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
}
