using FluentValidation;
using Phloem.Application.Features.Dishes.Commands;

namespace Phloem.Application.Features.Dishes.Validators;

public sealed class AddDishPhotoCommandValidator : AbstractValidator<AddDishPhotoCommand>
{
    /// <summary>Dish photos come straight from phone cameras, so allow more than profile photos.</summary>
    public const long MaxBytes = 10 * 1024 * 1024;   // 10 MB

    public AddDishPhotoCommandValidator()
    {
        RuleFor(x => x.DishId).NotEmpty();
        RuleFor(x => x.Content).NotNull();

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Please choose a photo.")
            .LessThanOrEqualTo(MaxBytes).WithMessage("Photos must be 10 MB or smaller.");
    }
}