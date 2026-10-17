using FluentValidation;
using Phloem.Application.Features.Shipments.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Input checks for <see cref="BookShipmentCommand"/>. Limits come from the Domain.</summary>
public sealed class BookShipmentCommandValidator : AbstractValidator<BookShipmentCommand>
{
    /// <summary>Route, phones, recipient, items, weight, and both tick boxes.</summary>
    public BookShipmentCommandValidator()
    {
        RuleFor(x => x.FromCountry).IsInEnum();
        RuleFor(x => x.ToCountry).IsInEnum()
            .NotEqual(x => x.FromCountry).WithMessage("Please choose two different countries.");

        RuleFor(x => x.SenderPhone).NotEmpty()
            .Must(PhoneNumber.IsValid).WithMessage(PhoneNumber.InvalidMessage);

        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(Shipment.RecipientNameMaxLength);
        RuleFor(x => x.RecipientPhone).NotEmpty()
            .Must(PhoneNumber.IsValid).WithMessage(PhoneNumber.InvalidMessage);
        RuleFor(x => x.RecipientAddressLine).NotEmpty().MaximumLength(Shipment.AddressLineMaxLength);
        RuleFor(x => x.RecipientCity).NotEmpty().MaximumLength(Shipment.CityMaxLength);
        RuleFor(x => x.RecipientRegionOrPostcode).NotEmpty().MaximumLength(Shipment.RegionOrPostcodeMaxLength);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Please add at least one item to your parcel.")
            .Must(items => items.Count <= Shipment.MaxItems)
            .WithMessage($"A parcel can have at most {Shipment.MaxItems} items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Category).IsInEnum();
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(ShipmentItem.DescriptionMaxLength);
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, ShipmentItem.MaxQuantity);
            item.RuleFor(i => i.UnitDeclaredValue).GreaterThanOrEqualTo(0);
        });

        RuleFor(x => x.Speed).IsInEnum();
        RuleFor(x => x.EstimatedWeightKg)
            .InclusiveBetween(Shipment.MinWeightKg, Shipment.MaxWeightKg)
            .WithMessage($"The weight must be between {Shipment.MinWeightKg} kg and {Shipment.MaxWeightKg} kg.");

        RuleFor(x => x.ConfirmedNoProhibitedItems).Equal(true)
            .WithMessage("Please confirm your parcel contains no prohibited items.");
        RuleFor(x => x.AcceptedTerms).Equal(true)
            .WithMessage("Please accept the shipping terms.");
    }
}
