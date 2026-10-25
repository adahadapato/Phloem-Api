using MediatR;
using Phloem.Application.Features.Orders.Common;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="GetShopOrdersQuery"/>.</summary>
public sealed class GetShopOrdersQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    IUserProfileRepository profiles,
    IAccountService accounts)
    : IRequestHandler<GetShopOrdersQuery, Result<PagedResult<ShopOrderSummaryDto>>>
{
    private const string UnknownUser = "Unknown user";

    /// <summary>Loads one page of the seller's orders, with customers' names fetched in ONE query.</summary>
    /// <returns>The page; or <see cref="VendorErrors.NotFound"/> if the user has no shop.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<PagedResult<ShopOrderSummaryDto>>> Handle(GetShopOrdersQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await orders.GetForVendorAsync(vendor.Id, query.Status, page, ct);

        var names = await profiles.GetFullNamesAsync(paged.Items.Select(o => o.CustomerId).Distinct().ToList(), ct);

        // Orders whose address had no phone: the customer's own (one lookup per customer on this page)
        var phones = new Dictionary<Guid, string?>();
        foreach (var customerId in paged.Items.Where(o => o.DeliveryPhone is null).Select(o => o.CustomerId).Distinct())
        {
            var contact = await accounts.GetContactAsync(customerId, ct);
            phones[customerId] = contact.IsSuccess ? contact.Value.PhoneNumber : null;
        }

        var items = paged.Items.Select(o => new ShopOrderSummaryDto(
            o.OrderNumber,
            names.GetValueOrDefault(o.CustomerId, UnknownUser),
            o.Status,
            o.PaidAtUtc,
            o.Lines.Sum(l => l.Quantity),
            LiveOrderPricing.ItemsSummary(o),
            o.Total,
            o.Currency,
            o.DeliveryWindow,
            o.DeliveryLabel,
            o.DeliveryAddressLine,
            o.DeliveryRecipientName ?? names.GetValueOrDefault(o.CustomerId, UnknownUser),
            o.DeliveryPhone ?? phones.GetValueOrDefault(o.CustomerId))).ToList();

        return new PagedResult<ShopOrderSummaryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}