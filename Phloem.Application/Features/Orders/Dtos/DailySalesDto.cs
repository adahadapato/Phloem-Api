namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>One bar in the seller dashboard's weekly sales chart. The app turns the date into "M", "T"…</summary>
/// <param name="Date">The day (UTC).</param>
/// <param name="Orders">How many paid orders that day (cancelled ones excluded).</param>
public sealed record DailySalesDto(DateOnly Date, int Orders);