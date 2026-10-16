using FluentValidation;
using Phloem.Application.Features.DeliveryPricings.Queries;

namespace Phloem.Application.Features.DeliveryPricings.Validators;

/// <summary>Input checks for <see cref="GetDeliveryWindowsQuery"/>.</summary>
public sealed class GetDeliveryWindowsQueryValidator : AbstractValidator<GetDeliveryWindowsQuery>
{
    /// <summary>Country must be a real value.</summary>
    public GetDeliveryWindowsQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
    }
}