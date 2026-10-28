using MediatR;
using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Deliveries.Queries;

/// <summary>Parcel staff → "Out for delivery": the couriers who can deliver in a country, by name.</summary>
/// <param name="Country">The parcel's destination country.</param>
public sealed record GetCouriersQuery(Country Country) : IRequest<Result<IReadOnlyList<CourierOptionDto>>>;
