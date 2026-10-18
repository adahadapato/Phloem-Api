using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

/// <summary>Logo and cover share the same limit.</summary>
public sealed class ChangeShopImageCommandValidator : AbstractValidator<ChangeShopImageCommand>
{
    /// <summary>Largest shop picture: 5 MB.</summary>
    public const long MaxBytes = 5 * 1024 * 1024;

    public ChangeShopImageCommandValidator()
    {
        RuleFor(x => x.Image).IsInEnum();
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Please choose a picture.")
            .LessThanOrEqualTo(MaxBytes).WithMessage("Pictures must be 5 MB or smaller.");
    }
}