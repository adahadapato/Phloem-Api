using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;
using System.Net;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>
/// Handles <see cref="SetDeliveryPricingCommand"/>:
/// 1. create or update the range; 2. move every shop's fee into it; 3. save;
/// 4. email each adjusted seller once. A failed email is logged and never undoes the change.
/// </summary>
public sealed class SetDeliveryPricingCommandHandler(
    IDeliveryPricingRepository pricings,
    IVendorRepository vendors,
    IEmailSender email,
    IUnitOfWork unitOfWork,
    ILogger<SetDeliveryPricingCommandHandler> logger)
    : IRequestHandler<SetDeliveryPricingCommand, Result<int>>
{
    /// <summary>Runs the steps above and returns the number of shops adjusted.</summary>
    public async Task<Result<int>> Handle(SetDeliveryPricingCommand command, CancellationToken ct)
    {
        // 1. Create the country's pricing the first time, otherwise change its range
        var pricing = await pricings.GetByCountryAsync(command.Country, ct);
        if (pricing is null)
        {
            pricing = DeliveryPricing.Create(command.Country, command.MinShopFee, command.MaxShopFee);
            pricings.Add(pricing);
        }
        else
        {
            pricing.SetShopFeeRange(command.MinShopFee, command.MaxShopFee);
        }

        // 2. Keep every shop's own fee inside the range (shops using the minimum follow it automatically)
        var shops = await vendors.GetByCountryAsync(command.Country, ct);
        var adjusted = new List<(Vendor Shop, DeliveryFeeAdjustment Change)>();

        foreach (var shop in shops)
        {
            var change = shop.ApplyDeliveryFeeRange(pricing);
            if (change is not null) adjusted.Add((shop, change));
        }

        // 3. Save the range and all adjusted fees together
        await unitOfWork.SaveChangesAsync(ct);

        // 4. Tell each adjusted seller
        foreach (var (shop, change) in adjusted)
            await TryEmailSellerAsync(shop, change, pricing, ct);

        return adjusted.Count;
    }

    /// <summary>Emails one seller about their adjusted fee; logs (doesn't throw) if sending fails.</summary>
    private async Task TryEmailSellerAsync(Vendor shop, DeliveryFeeAdjustment change, DeliveryPricing pricing,
                                           CancellationToken ct)
    {
        var body =
            $"<p>Hello {WebUtility.HtmlEncode(shop.Name)},</p>" +
            "<p>Phloem has updated the delivery fee range for your country to " +
            $"<strong>{pricing.MinShopFee:0.00}–{pricing.MaxShopFee:0.00} {pricing.Currency}</strong>.</p>" +
            $"<p>Your delivery fee was changed from <strong>{change.OldFee:0.00}</strong> to " +
            $"<strong>{change.NewFee:0.00} {change.Currency}</strong> so it stays within the range. " +
            "Your shop is still open and taking orders.</p>" +
            "<p>You can adjust it any time in Shop settings in the Phloem app.</p>";

        try
        {
            await email.SendAsync(shop.BusinessEmail.Value, "Your delivery fee has been updated", body, EmailReplyTo.Sellers, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Delivery-fee email to vendor {VendorId} failed", shop.Id);
        }
    }
}