using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>"Send a parcel" price preview: "7.2 kg → charged as 7.5 kg · GHS 417.50 · usually 3–5 days".</summary>
/// <param name="Speed">The chosen speed.</param>
/// <param name="EstimatedWeightKg">The weight the customer entered.</param>
/// <param name="ChargeableWeightKg">The weight rounded up to the next 0.5 kg.</param>
/// <param name="EstimatedPrice">Price for that weight. The final price is set when the parcel is weighed.</param>
/// <param name="Currency">Origin currency.</param>
/// <param name="MinTransitDays">Fewest days usually in transit.</param>
/// <param name="MaxTransitDays">Most days usually in transit.</param>
public sealed record ShipmentQuoteDto(
    DeliverySpeed Speed,
    decimal EstimatedWeightKg,
    decimal ChargeableWeightKg,
    decimal EstimatedPrice,
    Currency Currency,
    int MinTransitDays,
    int MaxTransitDays);
