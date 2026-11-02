using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// Admin → Orders → one order, with its timeline (full names). Permission <c>orders.view</c>; orders outside the
/// person's countries answer "not found". Every open is written to the audit log. Not cached.
/// </summary>
/// <param name="OrderNumber">The order number, e.g. "PH-261016-482193".</param>
public sealed record GetAdminOrderQuery(string OrderNumber) : IRequest<Result<AdminOrderDetailsDto>>;
