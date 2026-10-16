using MediatR;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Queries;

/// <summary>Shop settings → Delivery fee: the signed-in seller's fee and allowed range. Not cached.</summary>
public sealed record GetMyDeliveryFeeQuery : IRequest<Result<MyDeliveryFeeDto>>;