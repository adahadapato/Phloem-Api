using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>"My shipments": the signed-in sender's parcels, newest first. Never cached.</summary>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Shipments per page.</param>
public sealed record GetMyShipmentsQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ShipmentSummaryDto>>>;
