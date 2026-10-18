using FluentValidation;
using Phloem.Application.Features.Auth.Commands;

namespace Phloem.Application.Features.Auth.Validators;

/// <summary>Both values must be present (they come from the link, so a missing one means a damaged link).</summary>
public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    /// <summary>Sets up the rules.</summary>
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("This link is incomplete. Please use the link from the email.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("This link is incomplete. Please use the link from the email.");
    }
}