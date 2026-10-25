using FluentValidation;
using Phloem.Application.Features.ParcelItemRules.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.ParcelItemRules.Validators;

/// <summary>Input checks for <see cref="AddParcelItemRuleCommand"/>.</summary>
public sealed class AddParcelItemRuleCommandValidator : AbstractValidator<AddParcelItemRuleCommand>
{
    /// <summary>A real kind, a name, and an optional detail within their limits.</summary>
    public AddParcelItemRuleCommandValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ParcelItemRule.NameMaxLength);
        RuleFor(x => x.Detail).MaximumLength(ParcelItemRule.DetailMaxLength);
    }
}
