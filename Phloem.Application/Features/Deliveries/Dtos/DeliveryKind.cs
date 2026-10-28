namespace Phloem.Application.Features.Deliveries.Dtos;

/// <summary>What a courier is delivering.</summary>
public enum DeliveryKind
{
    /// <summary>A food order from a shop.</summary>
    Order = 1,

    /// <summary>A parcel sent from another country ("Send food abroad").</summary>
    Parcel = 2
}
