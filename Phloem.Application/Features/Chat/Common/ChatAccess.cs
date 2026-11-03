using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Chat.Common;

/// <summary>Who may use an order's chat: the customer, and the shop's owner, once the order is paid.</summary>
public sealed class ChatAccess(IOrderRepository orders, IVendorRepository vendors, IOrderChatRepository chats)
{
    /// <summary>The order, its chat (if started), the viewer's side and the shop's owner.</summary>
    /// <param name="Order">The order.</param>
    /// <param name="Chat">Its chat; null until the first message.</param>
    /// <param name="Role">The viewer's side.</param>
    /// <param name="ShopOwnerId">The shop's owner.</param>
    public sealed record Context(Order Order, OrderChat? Chat, ChatRole Role, Guid ShopOwnerId);

    /// <summary>Loads the order's chat for a participant.</summary>
    /// <param name="orderNumber">The order number.</param>
    /// <param name="userId">The person.</param>
    /// <param name="ct">Stops the queries.</param>
    /// <returns>The context; or <see cref="ChatErrors.NotFound"/> (unknown, unpaid, or not theirs: answered the same).</returns>
    public async Task<Result<Context>> ForParticipantAsync(string orderNumber, Guid userId, CancellationToken ct)
    {
        var order = await orders.GetByOrderNumberAsync(orderNumber.Trim(), ct);
        if (order is null || order.Status == OrderStatus.AwaitingPayment) return ChatErrors.NotFound;

        var chat = await chats.GetByOrderIdAsync(order.Id, ct);
        var ownerId = chat?.ShopOwnerId ?? (await vendors.GetByIdAsync(order.VendorId, ct))?.OwnerId;
        if (ownerId is null) return ChatErrors.NotFound;

        ChatRole? role = order.CustomerId == userId ? ChatRole.Customer : ownerId == userId ? ChatRole.Seller : null;
        if (role is null) return ChatErrors.NotFound;

        return new Context(order, chat, role.Value, ownerId.Value);
    }
}
