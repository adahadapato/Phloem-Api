using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="GetMyShipmentsQuery"/>.</summary>
public sealed class GetMyShipmentsQueryHandler(ICurrentUser currentUser, IShipmentRepository shipments)
    : IRequestHandler<GetMyShipmentsQuery, Result<PagedResult<ShipmentSummaryDto>>>
{
    /// <summary>One page of the sender's shipments.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>The page.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<PagedResult<ShipmentSummaryDto>>> Handle(GetMyShipmentsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await shipments.GetForSenderAsync(userId, page, ct);

        var items = paged.Items.Select(ShipmentSummaryDto.From).ToList();
        return new PagedResult<ShipmentSummaryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
