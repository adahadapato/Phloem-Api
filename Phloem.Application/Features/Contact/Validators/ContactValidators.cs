using FluentValidation;
using Phloem.Application.Features.Contact.Commands;
using Phloem.Application.Features.Contact.Queries;
using Phloem.Domain.Entities;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Contact.Validators;

/// <summary>Input checks for <see cref="SendContactMessageCommand"/> (the messages people see on the form).</summary>
public sealed class SendContactMessageCommandValidator : AbstractValidator<SendContactMessageCommand>
{
    public SendContactMessageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter your name.")
                            .MaximumLength(ContactMessage.NameMaxLength);
        RuleFor(x => x.Email).NotEmpty().WithMessage("Please enter your email address so we can reply.")
                             .EmailAddress().WithMessage("Please enter a valid email address.")
                             .MaximumLength(ContactMessage.EmailMaxLength);
        RuleFor(x => x.Topic).IsInEnum().WithMessage("Please choose what your message is about.");
        RuleFor(x => x.Reference).MaximumLength(ContactMessage.ReferenceMaxLength);
        RuleFor(x => x.Message).NotEmpty().WithMessage("Please write your message.")
                               .Must(m => m is null || m.Trim().Length >= ContactMessage.MessageMinLength)
                               .WithMessage($"Please tell us a little more (at least {ContactMessage.MessageMinLength} characters).")
                               .MaximumLength(ContactMessage.MessageMaxLength)
                               .WithMessage($"Please keep your message under {ContactMessage.MessageMaxLength:N0} characters.");
        RuleFor(x => x.Website).MaximumLength(200);
    }
}

/// <summary>Input checks for <see cref="UpdateContactMessageStatusCommand"/>.</summary>
public sealed class UpdateContactMessageStatusCommandValidator : AbstractValidator<UpdateContactMessageStatusCommand>
{
    public UpdateContactMessageStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(ContactMessage.NoteMaxLength)
                            .WithMessage($"Keep the note under {ContactMessage.NoteMaxLength} characters.");
    }
}

/// <summary>Input checks for <see cref="GetContactMessagesQuery"/>.</summary>
public sealed class GetContactMessagesQueryValidator : AbstractValidator<GetContactMessagesQuery>
{
    public GetContactMessagesQueryValidator()
    {
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status is not null);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
    }
}
