using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Validators;

/// <summary>Size and type are checked here and again on the real bytes in the handler.</summary>
public sealed class UploadShopDocumentCommandValidator : AbstractValidator<UploadShopDocumentCommand>
{
    public UploadShopDocumentCommandValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Please choose a file.")
            .LessThanOrEqualTo(DocumentFile.MaxFileSizeBytes).WithMessage(DocumentFile.TooLargeMessage);
    }
}