using System.Linq.Expressions;
using FluentValidation;
using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Deliveries.Validators;

/// <summary>The "Delivered" form's rules, shared by food orders and parcels.</summary>
internal static class DeliveryProofRules
{
    /// <summary>Largest photo (phones take big ones; it's shrunk to ≤ 1600 px when saved).</summary>
    public const long MaxPhotoBytes = 10 * 1024 * 1024;

    /// <summary>Photo required and ≤ 10 MB; a known handover; short texts; a name when left with a neighbour.</summary>
    /// <typeparam name="T">The command.</typeparam>
    public static void Apply<T>(AbstractValidator<T> validator,
                                Expression<Func<T, Stream>> photo,
                                Expression<Func<T, long>> photoSize,
                                Expression<Func<T, HandoverMethod>> handover,
                                Expression<Func<T, string?>> receivedBy,
                                Expression<Func<T, string?>> note)
    {
        validator.RuleFor(photo).NotNull().WithMessage("Please take a photo of where you left it.");

        validator.RuleFor(photoSize)
                 .GreaterThan(0).WithMessage("Please take a photo of where you left it.")
                 .LessThanOrEqualTo(MaxPhotoBytes).WithMessage("Photos must be 10 MB or smaller.");

        validator.RuleFor(handover).IsInEnum().WithMessage("Please choose how you handed it over.");

        validator.RuleFor(receivedBy).MaximumLength(ProofOfDelivery.ReceivedByMaxLength);

        var getHandover = handover.Compile();
        validator.RuleFor(receivedBy)
                 .Must(text => !string.IsNullOrWhiteSpace(text))
                 .When(x => getHandover(x) == HandoverMethod.Neighbour)
                 .WithMessage("Please say which neighbour took it, e.g. \"Mrs Ade at No. 14\".");

        validator.RuleFor(note).MaximumLength(ProofOfDelivery.NoteMaxLength);
    }
}
