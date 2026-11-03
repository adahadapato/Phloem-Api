using FluentValidation;
using Phloem.Application.Features.Chat.Commands;
using Phloem.Application.Features.Chat.Queries;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Chat.Validators;

/// <summary>Input checks for <see cref="SendChatMessageCommand"/>.</summary>
public sealed class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    /// <summary>Largest photo (shrunk to ≤ 1600 px when saved).</summary>
    public const long MaxPhotoBytes = 10 * 1024 * 1024;

    /// <summary>Text or a photo; text within its length; photo ≤ 10 MB.</summary>
    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(Order.OrderNumberMaxLength);
        RuleFor(x => x.Body).MaximumLength(ChatMessage.BodyMaxLength);
        RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.Body) || x.PhotoSizeBytes > 0)
                       .WithName("Body").WithMessage("Write a message or add a photo.");
        RuleFor(x => x.PhotoSizeBytes).LessThanOrEqualTo(MaxPhotoBytes).WithMessage("Photos must be 10 MB or smaller.");
    }
}

/// <summary>Input checks for <see cref="ReportChatCommand"/>.</summary>
public sealed class ReportChatCommandValidator : AbstractValidator<ReportChatCommand>
{
    /// <summary>A reason is required.</summary>
    public ReportChatCommandValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(Order.OrderNumberMaxLength);
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Tell us briefly what's wrong.")
                              .MaximumLength(OrderChat.ReportReasonMaxLength);
    }
}

/// <summary>Input checks for <see cref="GetOrderChatQuery"/>.</summary>
public sealed class GetOrderChatQueryValidator : AbstractValidator<GetOrderChatQuery>
{
    /// <summary>Order number required.</summary>
    public GetOrderChatQueryValidator() =>
        RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(Order.OrderNumberMaxLength);
}
