using FluentValidation;
using Phloem.Application.Features.Commissions.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Commissions.Validators;

/// <summary>Input checks for <see cref="SetCommissionCommand"/>.</summary>
public sealed class SetCommissionCommandValidator : AbstractValidator<SetCommissionCommand>
{
    /// <summary>Real country; rate between 0 and the Domain's maximum.</summary>
    public SetCommissionCommandValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.RatePercent)
            .InclusiveBetween(0m, Commission.MaxRatePercent)
            .WithMessage($"The commission must be between 0% and {Commission.MaxRatePercent}%.");
    }
}