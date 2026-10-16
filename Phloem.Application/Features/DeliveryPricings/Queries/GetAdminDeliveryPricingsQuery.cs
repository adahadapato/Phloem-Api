using MediatR;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Queries;

/// <summary>Admin → Delivery pricing page: every country's pricing. Not cached (admins need live data).</summary>
public sealed record GetAdminDeliveryPricingsQuery : IRequest<Result<IReadOnlyList<DeliveryPricingDto>>>;