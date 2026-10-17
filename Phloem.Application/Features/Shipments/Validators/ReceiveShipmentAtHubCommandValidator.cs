using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="ReceiveShipmentAtHubCommand"/>.</summary>
public sealed class ReceiveShipmentAtHubCommandValidator : AbstractValidator<ReceiveShipmentAtHubCommand>
{
    /// <summary>Tracking number and a weight inside the Domain's limits.</summary>
    public ReceiveShipmentAtHubCommandValidator()
    {
        TrackingNumberRules.Apply(RuleFor(x => x.TrackingNumber));
        RuleFor(x => x.WeighedWeightKg)
            .InclusiveBetween(Shipment.MinWeightKg, Shipment.MaxWeightKg)
            .WithMessage($"The weight must be between {Shipment.MinWeightKg} kg and {Shipment.MaxWeightKg} kg.");
    }
}
