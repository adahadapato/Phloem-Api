using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>
/// "Send a parcel" live price preview as the customer types a weight. Public (no sign-in needed),
/// not cached (cheap, and the inputs vary endlessly).
/// </summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="Speed">The chosen speed.</param>
/// <param name="EstimatedWeightKg">The weight the customer entered.</param>
public sealed record GetShipmentQuoteQuery(Country FromCountry, Country ToCountry, DeliverySpeed Speed,
                                           decimal EstimatedWeightKg) : IRequest<Result<ShipmentQuoteDto>>;
