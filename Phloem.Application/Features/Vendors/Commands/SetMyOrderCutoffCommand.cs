using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Shop settings → "Orders for today close at": after this time (the shop's local time) buyers can only choose
/// "Tomorrow" for delivery.
/// </summary>
/// <param name="Cutoff">e.g. 19:00.</param>
public sealed record SetMyOrderCutoffCommand(TimeOnly Cutoff) : IRequest<Result>;
