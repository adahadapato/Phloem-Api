using FluentValidation;
using Phloem.Application.Features.Shipments.Queries;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="GetStaffShipmentsQuery"/>.</summary>
public sealed class GetStaffShipmentsQueryValidator : AbstractValidator<GetStaffShipmentsQuery>
{
    /// <summary>Filters must be real values when given (null = no filter).</summary>
    public GetStaffShipmentsQueryValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum();
    }
}
