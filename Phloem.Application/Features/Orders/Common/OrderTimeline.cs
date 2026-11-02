using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Orders.Common;

/// <summary>
/// Turns an order's timeline into lines a viewer can read, with names.
/// </summary>
/// <remarks>
/// <para><b>Names are shown to everyone</b> (decided 2 Nov 2026, so customers and sellers can see who did each step,
/// e.g. who sent the order out):</para>
/// <list type="bullet">
///   <item><description><b>Staff</b> see full names and account Ids.</description></item>
///   <item><description><b>Customers and sellers</b> see first name and role: "Grace (customer)", "Kofi (Mama's
///   Kitchen)", "Yaw (courier)", "Ama (Phloem)". Staff full names aren't shown to the public.</description></item>
/// </list>
/// <para>Names are looked up now (one query), so a renamed account shows its current name.</para>
/// </remarks>
public sealed class OrderTimeline(IUserProfileRepository profiles)
{
    /// <summary>The order's timeline for a viewer, oldest first.</summary>
    /// <param name="order">The order, loaded with its timeline.</param>
    /// <param name="forStaff">True for staff (full names); false for the customer or the shop.</param>
    /// <param name="ct">Cancels the name lookup.</param>
    public async Task<IReadOnlyList<OrderEventDto>> BuildAsync(Order order, bool forStaff, CancellationToken ct)
    {
        var events = order.Events;
        if (events.Count == 0) return [];

        var ids = events.Select(e => e.ActorId).OfType<Guid>().Distinct().ToList();
        var names = await profiles.GetFullNamesAsync(ids, ct);

        return events.Select(e =>
        {
            var full = e.ActorId is { } id ? names.GetValueOrDefault(id, "Unknown user") : "Phloem";
            var role = RoleOf(e.ActorKind);
            var shown = forStaff || e.ActorKind == OrderActorKind.System
                ? full
                : $"{FirstName(full)} ({PublicRole(e.ActorKind, order.VendorName)})";

            return new OrderEventDto(e.Kind, e.StatusAfter, e.OccurredAtUtc, e.ActorKind, shown, role,
                                     forStaff ? e.ActorId : null, e.Note, e.IsRebuilt);
        }).ToList();
    }

    private static string RoleOf(OrderActorKind kind) => kind switch
    {
        OrderActorKind.Customer => "Customer",
        OrderActorKind.Seller => "Shop",
        OrderActorKind.Courier => "Courier",
        OrderActorKind.Staff => "Phloem staff",
        _ => "Phloem"
    };

    private static string PublicRole(OrderActorKind kind, string shopName) => kind switch
    {
        OrderActorKind.Customer => "customer",
        OrderActorKind.Seller => shopName,
        OrderActorKind.Courier => "courier",
        _ => "Phloem"
    };

    private static string FirstName(string fullName)
    {
        var trimmed = fullName.Trim();
        var space = trimmed.IndexOf(' ');
        return space > 0 ? trimmed[..space] : trimmed;
    }
}
