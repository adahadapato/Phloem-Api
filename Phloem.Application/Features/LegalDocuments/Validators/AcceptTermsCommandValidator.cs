using FluentValidation;
using Phloem.Application.Features.LegalDocuments.Commands;

namespace Phloem.Application.Features.LegalDocuments.Validators;

/// <summary>Input checks for <see cref="AcceptTermsCommand"/>.</summary>
public sealed class AcceptTermsCommandValidator : AbstractValidator<AcceptTermsCommand>
{
    public AcceptTermsCommandValidator() => RuleFor(x => x.Version).GreaterThanOrEqualTo(1);
}

/// <summary>Input checks for <see cref="AcknowledgePrivacyCommand"/>.</summary>
public sealed class AcknowledgePrivacyCommandValidator : AbstractValidator<AcknowledgePrivacyCommand>
{
    public AcknowledgePrivacyCommandValidator() => RuleFor(x => x.Version).GreaterThanOrEqualTo(1);
}
