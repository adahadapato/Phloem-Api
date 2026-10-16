namespace Phloem.Domain.Enums;

/// <summary>
/// When a LOCAL food order should arrive (the app's "Delivery time" choice).
/// Not to be confused with <see cref="DeliverySpeed"/>, which is for "Send food to the UK" shipments.
/// </summary>
/// <remarks>
/// Each country's <see cref="Entities.DeliveryPricing"/> decides which windows are offered and
/// what extra each one costs. Numbers are stored in the database: never change them after the
/// first migration; only add new values.
/// </remarks>
public enum DeliveryWindow
{
    /// <summary>As soon as possible (about 30–45 minutes).</summary>
    Asap = 1,

    /// <summary>Later today, in the evening.</summary>
    TodayEvening = 2,

    /// <summary>Tomorrow.</summary>
    Tomorrow = 3
}