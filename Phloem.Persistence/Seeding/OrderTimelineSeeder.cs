using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// Gives orders placed before timelines existed a timeline rebuilt from the dates they kept (placed, paid, sent out,
/// delivered, cancelled or declined), each line marked as rebuilt.
/// </summary>
/// <remarks>
/// <para>Safe to run on every start-up: only orders with no timeline at all are touched, so after the first run it
/// finds nothing. Works in batches so a large table doesn't use much memory.</para>
/// <para>Who did each rebuilt step is a best guess: the customer placed and paid, the shop's owner sent out or
/// declined, the courier delivered.</para>
/// </remarks>
internal static class OrderTimelineSeeder
{
    private const int BatchSize = 500;

    /// <summary>Rebuilds the missing timelines.</summary>
    /// <param name="context">The database.</param>
    /// <param name="logger">Start-up log.</param>
    /// <param name="cancellationToken">Stops seeding.</param>
    public static async Task SeedAsync(PhloemDbContext context, ILogger logger,
                                       CancellationToken cancellationToken = default)
    {
        var total = 0;
        while (true)
        {
            var orders = await context.Orders
                .Where(o => !o.Events.Any())
                .OrderBy(o => o.PlacedAtUtc)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
            if (orders.Count == 0) break;

            // Shop owners, including shops closed since (soft-deleted)
            var shopIds = orders.Select(o => o.VendorId).Distinct().ToList();
            var owners = await context.Vendors.IgnoreQueryFilters()
                .Where(v => shopIds.Contains(v.Id))
                .Select(v => new { v.Id, v.OwnerId })
                .ToDictionaryAsync(v => v.Id, v => v.OwnerId, cancellationToken);

            foreach (var order in orders)
                order.RebuildTimeline(owners.GetValueOrDefault(order.VendorId));

            await context.SaveChangesAsync(cancellationToken);
            total += orders.Count;

            if (orders.Count < BatchSize) break;
        }

        if (total > 0) logger.LogInformation("Rebuilt the timeline of {Count} older order(s)", total);
    }
}
