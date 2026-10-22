using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="CheckBasketQuery"/>.</summary>
public sealed class CheckBasketQueryHandler(
    IDishRepository dishes,
    IDeliveryPricingRepository pricings,
    IFileStorage storage,
    TimeProvider time)
    : IRequestHandler<CheckBasketQuery, Result<BasketCheckDto>>
{
    /// <summary>
    /// Loads the dishes in one query, groups them by shop, prices each line at today's price,
    /// and works out each shop's delivery fee for the chosen window.
    /// </summary>
    public async Task<Result<BasketCheckDto>> Handle(CheckBasketQuery query, CancellationToken ct)
    {
        var quantities = query.Items.ToDictionary(i => i.DishId, i => i.Quantity);
        var found = await dishes.GetByIdsAsync(quantities.Keys.ToList(), ct);

        var missing = quantities.Keys.Except(found.Select(d => d.Id)).ToList();
        var pricingByCountry = new Dictionary<Country, DeliveryPricing?>();
        var shops = new List<BasketShopDto>();
        var now = time.GetUtcNow().UtcDateTime;

        foreach (var shopDishes in found.GroupBy(d => d.VendorId))
        {
            var vendor = shopDishes.First().Vendor;

            var lines = shopDishes.Select(d =>
            {
                var available = d.CanBeOrdered;
                var file = d.PrimaryPhoto?.File;
                var imagePath = file?.ThumbnailPath ?? file?.Path;
                var qty = quantities[d.Id];

                return new BasketLineDto(d.Id, d.Name,
                    imagePath is null ? null : storage.GetPublicUrl(imagePath),
                    qty, d.Price, available ? d.Price * qty : 0, available);
            }).ToList();

            var subtotal = lines.Sum(l => l.LineTotal);

            // Delivery fee for this shop (one pricing lookup per country, not per shop)
            if (!pricingByCountry.TryGetValue(vendor.Country, out var pricing))
                pricingByCountry[vendor.Country] = pricing = await pricings.GetByCountryAsync(vendor.Country, ct);

            decimal? fee = null;
            string? problem;

            // Delivery times the shop takes right now (its cut-off), among those offered in its country
            IReadOnlyList<DeliveryWindow> open = Enum.GetValues<DeliveryWindow>()
                .Where(w => pricing is not null && pricing.Offers(w) && vendor.AcceptsDeliveryWindow(w, now))
                .ToList();

            if (pricing is null)
            {
                problem = DeliveryPricingErrors.NotSet.Message;
            }
            else if (vendor.CheckDeliveryWindow(query.DeliveryWindow, now) is { IsFailure: true } closed)
            {
                problem = closed.Error.Message;   // after the cut-off: "… Please choose Tomorrow"
            }
            else
            {
                var priced = pricing.FeeFor(vendor, query.DeliveryWindow);
                fee = priced.IsSuccess ? priced.Value : null;
                problem = priced.IsSuccess ? null : priced.Error.Message;
            }

            shops.Add(new BasketShopDto(vendor.Id, vendor.Name, vendor.Currency, lines, subtotal,
                                        fee, problem, fee is null ? null : subtotal + fee,
                                        open, vendor.OrderCutoffTime));
        }

        var canPlace = missing.Count == 0
                       && shops.All(s => s.DeliveryFee is not null && s.Lines.All(l => l.IsAvailable));

        return new BasketCheckDto(shops, missing, canPlace);
    }
}