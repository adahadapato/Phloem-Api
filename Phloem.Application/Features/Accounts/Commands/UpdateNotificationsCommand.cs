using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Notifications page: the app sends all 5 switches each time one is flipped.</summary>
public sealed record UpdateNotificationsCommand(
    bool OrderUpdates,
    bool ShipmentUpdates,
    bool Offers,
    bool NewDishes,
    bool ShopOrders) : IRequest<Result>;