using FluentValidation;
using Phloem.Application.Features.Auth.Commands;

namespace Phloem.Application.Features.Auth.Validators;

/// <summary>Checks the shape of <see cref="ExchangeLoginCodeCommand"/>; whether the code is real is the handler's job.</summary>
public sealed class ExchangeLoginCodeCommandValidator : AbstractValidator<ExchangeLoginCodeCommand>
{
    /// <summary>Longest code accepted. Real codes are 43 characters; anything far longer is junk.</summary>
    private const int CodeMaxLength = 100;

    /// <summary>Sets up the rules.</summary>
    public ExchangeLoginCodeCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(CodeMaxLength);
    }
}
