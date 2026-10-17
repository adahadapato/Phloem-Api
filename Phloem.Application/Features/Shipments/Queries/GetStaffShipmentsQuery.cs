using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>
/// Staff (ShipmentManager) → Shipments list, newest first, with optional filters
/// (e.g. Booked + from Ghana = "parcels expected at the Accra hub"). Not cached.
/// </summary>
/// <param name="Status">Only this status; null = all.</param>
/// <param name="FromCountry">Only parcels from this country; null = all.</param>
/// <param name="ToCountry">Only parcels to this country; null = all.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Shipments per page.</param>
public sealed record GetStaffShipmentsQuery(ShipmentStatus? Status, Country? FromCountry, Country? ToCountry,
                                            int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<StaffShipmentSummaryDto>>>;
