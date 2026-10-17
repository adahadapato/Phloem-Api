using FluentValidation;
using Phloem.Application.Features.Shipments.Queries;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="GetMyShipmentQuery"/>.</summary>
public sealed class GetMyShipmentQueryValidator : AbstractValidator<GetMyShipmentQuery>
{
    /// <summary>Tracking number required.</summary>
    public GetMyShipmentQueryValidator() => TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
}
