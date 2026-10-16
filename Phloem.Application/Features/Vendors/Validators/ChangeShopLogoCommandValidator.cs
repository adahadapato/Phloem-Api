using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class ChangeShopLogoCommandValidator : AbstractValidator<ChangeShopLogoCommand>
{
    public const long MaxBytes = 5 * 1024 * 1024;   // 5 MB

    public ChangeShopLogoCommandValidator()
    {
        RuleFor(x => x.Content).NotNull();

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Please choose a logo.")
            .LessThanOrEqualTo(MaxBytes).WithMessage("Logos must be 5 MB or smaller.");
    }
}