using FluentValidation;


using Phloem.Application.Features.Accounts.Commands;


/// <summary>Input checks for <see cref="EnableTwoFactorCommand"/>.</summary>
public sealed class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
    /// <summary>A 6-digit code (spaces allowed, e.g. "123 456").</summary>
    public EnableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Must(code => code.Replace(" ", string.Empty) is { Length: 6 } digits && digits.All(char.IsAsciiDigit))
            .WithMessage("Please enter the 6-digit code from your authenticator app.");
    }
}