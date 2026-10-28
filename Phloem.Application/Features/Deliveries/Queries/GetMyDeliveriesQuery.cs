using MediatR;
using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Deliveries.Queries;

/// <summary>
/// Courier → "My deliveries": food orders and parcels given to the signed-in person. Works for couriers and for
/// sellers delivering their own orders (anything whose courier is the caller).
/// </summary>
/// <param name="Done">False = still to deliver (oldest first); true = delivered in the last
/// <see cref="GetMyDeliveriesQueryHandler.DoneWindowDays"/> days (newest first).</param>
public sealed record GetMyDeliveriesQuery(bool Done = false) : IRequest<Result<IReadOnlyList<DeliveryJobDto>>>;
