using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>
/// One step on the tracking timeline, e.g. "Received at hub · 17 Oct, 10:42 · Weighed: 7.40 kg".
/// </summary>
/// <param name="Status">The status reached at this step.</param>
/// <param name="OccurredAt">When it happened (UTC); the app shows it in local time.</param>
/// <param name="Note">Detail under the step; always null in PUBLIC tracking.</param>
/// <remarks>
/// Two factories, so it's always clear which view is being built: notes can contain names
/// ("Signed for by K. Boateng"), so public tracking must never include them.
/// </remarks>
public sealed record ShipmentEventDto(ShipmentStatus Status, DateTime OccurredAt, string? Note)
{
    /// <summary>For the sender and staff: includes the note.</summary>
    /// <param name="e">The timeline step.</param>
    /// <returns>The DTO with its note.</returns>
    public static ShipmentEventDto WithNote(ShipmentEvent e) => new(e.Status, e.OccurredAtUtc, e.Note);

    /// <summary>For public tracking: the note is left out.</summary>
    /// <param name="e">The timeline step.</param>
    /// <returns>The DTO without a note.</returns>
    public static ShipmentEventDto WithoutNote(ShipmentEvent e) => new(e.Status, e.OccurredAtUtc, null);
}
