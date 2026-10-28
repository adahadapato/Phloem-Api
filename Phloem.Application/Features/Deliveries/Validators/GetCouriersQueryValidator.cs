using FluentValidation;
using Phloem.Application.Features.Deliveries.Queries;

namespace Phloem.Application.Features.Deliveries.Validators;

/// <summary>Input checks for <see cref="GetCouriersQuery"/>.</summary>
public sealed class GetCouriersQueryValidator : AbstractValidator<GetCouriersQuery>
{
    /// <summary>A real country.</summary>
    public GetCouriersQueryValidator() =>
        RuleFor(x => x.Country).IsInEnum().WithMessage("Choose a country.");
}
