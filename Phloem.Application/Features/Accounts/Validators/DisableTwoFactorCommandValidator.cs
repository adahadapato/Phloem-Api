using FluentValidation;


using Phloem.Application.Features.Accounts.Commands;


/// <summary>Input checks for <see cref="DisableTwoFactorCommand"/>.</summary>
public sealed class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    /// <summary>Password required.</summary>
    public DisableTwoFactorCommandValidator() =>
        RuleFor(x => x.Password).NotEmpty().WithMessage("Please enter your password to confirm.");
}