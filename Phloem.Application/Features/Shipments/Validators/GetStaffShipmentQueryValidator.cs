using FluentValidation;
using Phloem.Application.Features.Shipments.Queries;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="GetStaffShipmentQuery"/>.</summary>
public sealed class GetStaffShipmentQueryValidator : AbstractValidator<GetStaffShipmentQuery>
{
    /// <summary>Tracking number required.</summary>
    public GetStaffShipmentQueryValidator() => TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
}
