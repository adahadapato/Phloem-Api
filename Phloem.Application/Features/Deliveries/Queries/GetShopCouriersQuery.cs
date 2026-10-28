using MediatR;
using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Deliveries.Queries;

/// <summary>
/// Seller → "Send out": who can take this order. First "I'll deliver it myself" (the seller), then the couriers in the
/// shop's country, by name.
/// </summary>
public sealed record GetShopCouriersQuery : IRequest<Result<IReadOnlyList<CourierOptionDto>>>;
