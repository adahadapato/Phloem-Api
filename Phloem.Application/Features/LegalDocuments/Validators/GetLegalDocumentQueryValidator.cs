using FluentValidation;
using Phloem.Application.Features.LegalDocuments.Queries;

namespace Phloem.Application.Features.LegalDocuments.Validators;

/// <summary>Input checks for <see cref="GetLegalDocumentQuery"/>.</summary>
public sealed class GetLegalDocumentQueryValidator : AbstractValidator<GetLegalDocumentQuery>
{
    /// <summary>A real document type.</summary>
    public GetLegalDocumentQueryValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
    }
}
