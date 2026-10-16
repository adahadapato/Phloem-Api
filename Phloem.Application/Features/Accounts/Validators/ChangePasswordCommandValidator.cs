using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;

namespace Phloem.Application.Features.Accounts.Validators;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>Keep the SAME as Identity's RequiredLength (set later in Infrastructure).</summary>
    public const int MinLength = 8;

    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(MinLength)
            .NotEqual(x => x.CurrentPassword)
                .WithMessage("Your new password must be different from your current one.");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
                .WithMessage("The passwords don't match.");
    }
}