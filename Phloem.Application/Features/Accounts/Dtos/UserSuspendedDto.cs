namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>What suspending someone did, so the admin sees it straight away.</summary>
/// <param name="ShopSuspended">True when the person owns a shop (now hidden with them).</param>
/// <param name="CancelledShopOrders">Their shop's orders that hadn't left it yet, now cancelled.</param>
/// <param name="OrdersToRefund">Of those, how many were paid (flagged for a refund).</param>
public sealed record UserSuspendedDto(bool ShopSuspended, int CancelledShopOrders, int OrdersToRefund);
