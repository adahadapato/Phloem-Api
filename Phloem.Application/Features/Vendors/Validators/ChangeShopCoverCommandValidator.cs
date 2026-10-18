using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

/// <summary>Same limit as the logo.</summary>
public sealed class ChangeShopCoverCommandValidator : AbstractValidator<ChangeShopCoverCommand>
{
    public ChangeShopCoverCommandValidator()
    {
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Please choose a cover photo.")
            .LessThanOrEqualTo(ChangeShopImageCommandValidator.MaxBytes).WithMessage("Cover photos must be 5 MB or smaller.");
    }
}