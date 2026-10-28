using MediatR;
using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Application.Features.Orders.Common;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Deliveries.Queries;

/// <summary>Handles <see cref="GetMyDeliveriesQuery"/>.</summary>
public sealed class GetMyDeliveriesQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IShipmentRepository shipments,
    IVendorRepository vendors,
    IUserProfileRepository profiles,
    IAccountService accounts,
    TimeProvider time)
    : IRequestHandler<GetMyDeliveriesQuery, Result<IReadOnlyList<DeliveryJobDto>>>
{
    /// <summary>"Done" shows deliveries from this many days back.</summary>
    public const int DoneWindowDays = 7;

    /// <summary>Most cards of each kind (orders, parcels) in one list.</summary>
    public const int MaxPerKind = 50;

    private const string UnknownUser = "Unknown user";
    private const string Hub = "Phloem hub";

    /// <summary>Orders and parcels whose courier is the caller, food first then parcels, each in delivery order.</summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<IReadOnlyList<DeliveryJobDto>>> Handle(GetMyDeliveriesQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        DateTime? since = query.Done ? time.GetUtcNow().UtcDateTime.AddDays(-DoneWindowDays) : null;

        var myOrders = await orders.GetForCourierAsync(userId, since, MaxPerKind, ct);
        var myParcels = await shipments.GetForCourierAsync(userId, since, MaxPerKind, ct);

        var jobs = new List<DeliveryJobDto>(myOrders.Count + myParcels.Count);
        jobs.AddRange(await OrderJobsAsync(myOrders, ct));
        jobs.AddRange(myParcels.Select(ParcelJob));

        // Still to deliver: oldest first. Done: newest first.
        IReadOnlyList<DeliveryJobDto> sorted = query.Done
            ? jobs.OrderByDescending(j => j.DeliveredAt).ToList()
            : jobs.OrderBy(j => j.AssignedAt).ToList();
        return Result<IReadOnlyList<DeliveryJobDto>>.Success(sorted);
    }

    /// <summary>Food cards: the customer's name and phone, and the shop's phone (a few lookups for a short list).</summary>
    private async Task<IEnumerable<DeliveryJobDto>> OrderJobsAsync(IReadOnlyList<Order> list, CancellationToken ct)
    {
        if (list.Count == 0) return [];

        var names = await profiles.GetFullNamesAsync(list.Select(o => o.CustomerId).Distinct().ToList(), ct);

        var phones = new Dictionary<Guid, string?>();
        foreach (var customerId in list.Where(o => o.DeliveryPhone is null).Select(o => o.CustomerId).Distinct())
        {
            var contact = await accounts.GetContactAsync(customerId, ct);
            phones[customerId] = contact.IsSuccess ? contact.Value.PhoneNumber : null;
        }

        var shopPhones = new Dictionary<Guid, string?>();
        foreach (var vendorId in list.Select(o => o.VendorId).Distinct())
            shopPhones[vendorId] = (await vendors.GetByIdAsync(vendorId, ct))?.BusinessPhone?.Value;

        return list.Select(o => new DeliveryJobDto(
            DeliveryKind.Order,
            o.OrderNumber,
            o.VendorName,
            shopPhones.GetValueOrDefault(o.VendorId),
            o.DeliveryRecipientName ?? names.GetValueOrDefault(o.CustomerId, UnknownUser),
            o.DeliveryPhone ?? phones.GetValueOrDefault(o.CustomerId),
            o.DeliveryAddressLine,
            o.DeliveryCountry,
            LiveOrderPricing.ItemsSummary(o),
            o.DeliveryWindow,
            o.CourierAssignedAtUtc,
            o.DeliveredAtUtc,
            o.DeliveryProof?.Describe()));
    }

    /// <summary>Parcel cards: everything is on the shipment already.</summary>
    private static DeliveryJobDto ParcelJob(Shipment s)
    {
        var count = s.Items.Sum(i => i.Quantity);
        var weight = s.WeighedWeightKg ?? s.EstimatedWeightKg;
        var items = $"{count} item{(count == 1 ? "" : "s")}, {weight:0.0} kg";

        return new DeliveryJobDto(
            DeliveryKind.Parcel,
            s.TrackingNumber,
            Hub,
            null,
            s.RecipientName,
            s.RecipientPhone.Value,
            $"{s.RecipientAddressLine}, {s.RecipientCity}, {s.RecipientRegionOrPostcode}",
            s.ToCountry,
            items,
            null,
            s.CourierAssignedAtUtc,
            s.DeliveredAtUtc,
            s.DeliveryProof?.Describe());
    }
}
