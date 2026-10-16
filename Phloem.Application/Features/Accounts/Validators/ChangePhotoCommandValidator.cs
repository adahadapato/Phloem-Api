using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;

namespace Phloem.Application.Features.Accounts.Validators;

public sealed class ChangePhotoCommandValidator : AbstractValidator<ChangePhotoCommand>
{
    public const long MaxBytes = 5 * 1024 * 1024;   // 5 MB

    public ChangePhotoCommandValidator()
    {
        RuleFor(x => x.Content).NotNull();

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Please choose a photo.")
            .LessThanOrEqualTo(MaxBytes).WithMessage("Photos must be 5 MB or smaller.");
    }
}