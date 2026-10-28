using FluentValidation;
using Phloem.Application.Features.Deliveries.Validators;
using Phloem.Application.Features.Orders.Commands;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="MarkOrderDeliveredCommand"/>.</summary>
public sealed class MarkOrderDeliveredCommandValidator : AbstractValidator<MarkOrderDeliveredCommand>
{
    /// <summary>Order number required, plus the delivery photo and handover rules.</summary>
    public MarkOrderDeliveredCommandValidator()
    {
        OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
        DeliveryProofRules.Apply(this, x => x.Photo, x => x.PhotoSizeBytes, x => x.Handover, x => x.ReceivedBy,
                                 x => x.Note);
    }
}
