using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Accounts.Validators;

/// <summary>Input checks for <see cref="SuspendUserCommand"/> (the end date is checked by the Domain).</summary>
public sealed class SuspendUserCommandValidator : AbstractValidator<SuspendUserCommand>
{
    public SuspendUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Please say why (the person will see it).")
            .MaximumLength(UserProfile.SuspensionReasonMaxLength);
    }
}
