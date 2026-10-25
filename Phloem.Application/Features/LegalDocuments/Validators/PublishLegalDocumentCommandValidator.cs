using FluentValidation;
using Phloem.Application.Features.LegalDocuments.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.LegalDocuments.Validators;

/// <summary>Input checks for <see cref="PublishLegalDocumentCommand"/>.</summary>
public sealed class PublishLegalDocumentCommandValidator : AbstractValidator<PublishLegalDocumentCommand>
{
    /// <summary>A real type, a title and the text, within their limits.</summary>
    public PublishLegalDocumentCommandValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(LegalDocument.TitleMaxLength);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(LegalDocument.ContentMaxLength);
    }
}
