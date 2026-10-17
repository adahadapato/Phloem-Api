using FluentValidation;


using Phloem.Application.Features.Accounts.Commands;


/// <summary>Input checks for <see cref="RegenerateRecoveryCodesCommand"/>.</summary>
public sealed class RegenerateRecoveryCodesCommandValidator : AbstractValidator<RegenerateRecoveryCodesCommand>
{
    /// <summary>Password required.</summary>
    public RegenerateRecoveryCodesCommandValidator() =>
        RuleFor(x => x.Password).NotEmpty().WithMessage("Please enter your password to confirm.");
}