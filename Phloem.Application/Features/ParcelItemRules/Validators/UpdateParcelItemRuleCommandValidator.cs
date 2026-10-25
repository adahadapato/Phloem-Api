using FluentValidation;
using Phloem.Application.Features.ParcelItemRules.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.ParcelItemRules.Validators;

/// <summary>Input checks for <see cref="UpdateParcelItemRuleCommand"/>.</summary>
public sealed class UpdateParcelItemRuleCommandValidator : AbstractValidator<UpdateParcelItemRuleCommand>
{
    /// <summary>An id, a real kind, a name, and an optional detail within their limits.</summary>
    public UpdateParcelItemRuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ParcelItemRule.NameMaxLength);
        RuleFor(x => x.Detail).MaximumLength(ParcelItemRule.DetailMaxLength);
    }
}
