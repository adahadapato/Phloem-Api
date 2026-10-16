using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using System.Net;

namespace Phloem.Application.Features.DishCatalogs.Commands;

/// <summary>
/// 1. Set the range.  2. Re-check every dish with that name in that country (flag / un-flag).
/// 3. Save.  4. Email each affected seller ONCE, listing all their newly flagged dishes.
/// Emails go out AFTER the save, and a failed email never undoes the admin's change.
/// </summary>
public sealed class SetDishPriceRangeCommandHandler(
    IDishCatalogRepository catalog,
    IDishRepository dishes,
    IEmailSender email,
    IUnitOfWork unitOfWork,
    ILogger<SetDishPriceRangeCommandHandler> logger)
    : IRequestHandler<SetDishPriceRangeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SetDishPriceRangeCommand command, CancellationToken ct)
    {
        var item = await catalog.GetByIdAsync(command.DishCatalogId, ct);
        if (item is null) return DishCatalogErrors.NotFound;

        // 1. The range (Domain rule: only for approved names)
        var set = item.SetPriceRange(command.Country, command.MinPrice, command.MaxPrice);
        if (set.IsFailure) return set.Error;
        var range = set.Value;

        // 2. Re-check this name's dishes in this country
        var affected = await dishes.GetByCatalogAndCountryAsync(item.Id, command.Country, ct);
        var newlyFlagged = affected.Where(dish => dish.ApplyPriceRange(range)).ToList();

        // 3. Save the range and every flag change together
        await unitOfWork.SaveChangesAsync(ct);

        // 4. One email per seller
        foreach (var shop in newlyFlagged.GroupBy(d => d.Vendor))
            await TryEmailSellerAsync(shop.Key, shop.ToList(), range, ct);

        return newlyFlagged.Count;
    }

    private async Task TryEmailSellerAsync(Vendor vendor, IReadOnlyList<Dish> flagged, DishPriceRange range,
                                           CancellationToken ct)
    {
        var dishList = string.Join("", flagged.Select(d =>
            $"<li>{WebUtility.HtmlEncode(d.Name)}: currently {d.Price:0.00} {d.Currency}</li>"));

        var subject = flagged.Count == 1
            ? "Action needed: 1 dish needs a new price"
            : $"Action needed: {flagged.Count} dishes need a new price";

        var body =
            $"<p>Hello {WebUtility.HtmlEncode(vendor.Name)},</p>" +
            $"<p>Phloem has updated the allowed price range to " +
            $"<strong>{range.MinPrice:0.00}–{range.MaxPrice:0.00} {range.Currency}</strong>. " +
            "These dishes are now outside it and <strong>can't be ordered</strong> until you update their price:</p>" +
            $"<ul>{dishList}</ul>" +
            "<p>Open your Seller dashboard in the Phloem app to update them.</p>";

        try
        {
            await email.SendAsync(vendor.BusinessEmail.Value, subject, body, ct);
        }
        catch (Exception ex)
        {
            // The dashboard banner still tells the seller, so a failed email isn't fatal
            logger.LogWarning(ex, "Price-range email to vendor {VendorId} failed", vendor.Id);
        }
    }
}