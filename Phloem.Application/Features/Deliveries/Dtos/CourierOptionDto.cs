namespace Phloem.Application.Features.Deliveries.Dtos;

/// <summary>One entry in a "Choose a courier" list.</summary>
/// <param name="Id">The courier's user Id (what the send-out request takes).</param>
/// <param name="Name">Their full name.</param>
/// <param name="IsMe">True for the seller's own "I'll deliver it myself" entry.</param>
public sealed record CourierOptionDto(Guid Id, string Name, bool IsMe);
