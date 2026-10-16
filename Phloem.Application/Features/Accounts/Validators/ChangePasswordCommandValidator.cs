using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;

namespace Phloem.Application.Features.Accounts.Validators;

/// <summary>
/// Only checks what Identity can't: fields filled in, new ≠ current, and the two new ones match.
/// Password strength (length, digits…) is configured in Identity; its message comes back as a Result.
/// </summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .NotEqual(x => x.CurrentPassword)
                .WithMessage("Your new password must be different from your current one.");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
                .WithMessage("The passwords don't match.");
    }
}