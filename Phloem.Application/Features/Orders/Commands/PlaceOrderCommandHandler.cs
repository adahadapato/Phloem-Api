using Phloem.Application.Common.Terms;
using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Handles <see cref="PlaceOrderCommand"/>.</summary>
public sealed class PlaceOrderCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IDishRepository dishes,
    IDeliveryPricingRepository pricings,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    TermsGate terms)
    : IRequestHandler<PlaceOrderCommand, Result<IReadOnlyList<PlacedOrderDto>>>
{
    /// <summary>How many times to try for an unused order number before giving up (a bug if reached).</summary>
    private const int MaxOrderNumberAttempts = 10;

    /// <summary>
    /// 1. Load the customer, address and dishes. 2. Split by shop. 3. Price each shop's delivery.
    /// 4. <see cref="Order.Place"/> for each (all rules in Domain). 5. Save all at once.
    /// </summary>
    /// <returns>
    /// The new orders; or the first failure: <see cref="TermsErrors.NotAccepted"/>, <see cref="UserProfileErrors.NotFound"/>,
    /// <see cref="UserProfileErrors.AddressNotFound"/>, <see cref="DishErrors.NotFound"/>,
    /// <see cref="DeliveryPricingErrors.NotSet"/>, <see cref="DeliveryPricingErrors.WindowNotOffered"/>,
    /// or any <see cref="OrderErrors"/> from Order.Place. Nothing is saved on failure.
    /// </returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user, or no free order number.</exception>
    public async Task<Result<IReadOnlyList<PlacedOrderDto>>> Handle(PlaceOrderCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        // 0. The latest Terms must be accepted first ("We've updated our Terms" on the website / app)
        var accepted = await terms.EnsureAcceptedAsync(userId, ct);
        if (accepted.IsFailure) return accepted.Error;

        // 1. Customer, address, dishes
        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var address = profile.Addresses.FirstOrDefault(a => a.Id == command.DeliveryAddressId);
        if (address is null) return UserProfileErrors.AddressNotFound;

        var quantities = command.Items.ToDictionary(i => i.DishId, i => i.Quantity);
        var found = await dishes.GetByIdsAsync(quantities.Keys.ToList(), ct);
        if (found.Count != quantities.Count) return DishErrors.NotFound;   // a dish no longer exists

        // 2–4. One order per shop
        var now = time.GetUtcNow().UtcDateTime;
        var usedNumbers = new HashSet<string>();
        var pricingByCountry = new Dictionary<Country, DeliveryPricing?>();
        var placed = new List<Order>();

        foreach (var shopDishes in found.GroupBy(d => d.VendorId))
        {
            var vendor = shopDishes.First().Vendor;

            if (!pricingByCountry.TryGetValue(vendor.Country, out var pricing))
                pricingByCountry[vendor.Country] = pricing = await pricings.GetByCountryAsync(vendor.Country, ct);
            if (pricing is null) return DeliveryPricingErrors.NotSet;

            var fee = pricing.FeeFor(vendor, command.DeliveryWindow);
            if (fee.IsFailure) return fee.Error;

            var number = await NewOrderNumberAsync(now, usedNumbers, ct);

            var order = Order.Place(number, profile, address,
                                    shopDishes.Select(d => (d, quantities[d.Id])),
                                    fee.Value, command.DeliveryWindow, now);
            if (order.IsFailure) return order.Error;

            placed.Add(order.Value);
        }

        // 5. All orders in one save = one database transaction
        foreach (var order in placed)
        {
            orders.Add(order);
            // "By placing your order you agree…": the Terms version, kept with each order number
            await terms.RecordAsync(userId, LegalDocumentType.Terms, AcceptanceContext.Checkout, order.OrderNumber, ct);
        }
        await unitOfWork.SaveChangesAsync(ct);

        IReadOnlyList<PlacedOrderDto> result = placed.Select(PlacedOrderDto.From).ToList();
        return Result<IReadOnlyList<PlacedOrderDto>>.Success(result);
    }

    /// <summary>
    /// "PH-yyMMdd-######" (date + 6 random digits), unused in the database AND in this request.
    /// The database's unique index is the final guard.
    /// </summary>
    private async Task<string> NewOrderNumberAsync(DateTime utcNow, HashSet<string> usedInThisRequest,
                                                   CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxOrderNumberAttempts; attempt++)
        {
            var number = $"PH-{utcNow:yyMMdd}-{Random.Shared.Next(100_000, 1_000_000)}";
            if (usedInThisRequest.Contains(number)) continue;
            if (await orders.OrderNumberExistsAsync(number, ct)) continue;

            usedInThisRequest.Add(number);
            return number;
        }

        throw new InvalidOperationException("Couldn't generate a unique order number.");
    }
}