using FluentValidation;
using Phloem.Application.Features.Shipments.Queries;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="GetShipmentQuoteQuery"/>.</summary>
public sealed class GetShipmentQuoteQueryValidator : AbstractValidator<GetShipmentQuoteQuery>
{
    /// <summary>Two different real countries, a real speed, and a weight inside the Domain's limits.</summary>
    public GetShipmentQuoteQueryValidator()
    {
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum()
            .NotEqual(x => x.FromCountry).WithMessage("Please choose two different countries.");
        RuleFor(x => x.Speed).IsInEnum();
        RuleFor(x => x.EstimatedWeightKg)
            .InclusiveBetween(Shipment.MinWeightKg, Shipment.MaxWeightKg)
            .WithMessage($"The weight must be between {Shipment.MinWeightKg} kg and {Shipment.MaxWeightKg} kg.");
    }
}
