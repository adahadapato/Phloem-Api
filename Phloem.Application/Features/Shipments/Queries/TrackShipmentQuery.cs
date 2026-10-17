using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>
/// PUBLIC "Track a parcel" (no sign-in), e.g. for the recipient. Returns the limited view only.
/// Presentation must rate-limit this endpoint so tracking numbers can't be guessed in bulk.
/// </summary>
/// <param name="TrackingNumber">The tracking number.</param>
public sealed record TrackShipmentQuery(string TrackingNumber) : IRequest<Result<PublicTrackingDto>>;
