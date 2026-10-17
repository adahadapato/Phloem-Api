using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="GetStaffShipmentsQuery"/>.</summary>
public sealed class GetStaffShipmentsQueryHandler(IShipmentRepository shipments)
    : IRequestHandler<GetStaffShipmentsQuery, Result<PagedResult<StaffShipmentSummaryDto>>>
{
    /// <summary>One filtered page of shipments (sender and recipient names are on the shipment itself).</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>The page.</returns>
    public async Task<Result<PagedResult<StaffShipmentSummaryDto>>> Handle(GetStaffShipmentsQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await shipments.GetPagedAsync(query.Status, query.FromCountry, query.ToCountry, page, ct);

        var items = paged.Items.Select(StaffShipmentSummaryDto.From).ToList();
        return new PagedResult<StaffShipmentSummaryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
