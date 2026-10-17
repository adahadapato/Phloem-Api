namespace Phloem.Application.Common.Caching;


/// <summary>
/// The names of every cache tag, kept in one place so a typo can't break cache clearing.
/// </summary>
/// <remarks>
/// <para><b>How tags work.</b> A query marked <c>[Cacheable(CacheTags.X)]</c> stores its result under
/// tag X. A command marked <c>[InvalidatesCache(CacheTags.X)]</c> removes EVERY cached result with
/// tag X after it succeeds, so the next read gets fresh data. One tag can cover many queries
/// (e.g. every page and filter of the dish listing).</para>
///
/// <para><b>When adding a new cached query:</b> reuse the tag for the data it reads, or add a new
/// constant here. Then make sure EVERY command that changes that data invalidates the tag,
/// otherwise users will see stale data until the cache expires.</para>
///
/// <para><b>Never cache:</b> orders, balances, admin screens or checkout. Those must always be live.</para>
///
/// <para>The values are plain strings stored by HybridCache; changing a value only empties that
/// tag's cache once, so it's safe.</para>
/// </remarks>
public static class CacheTags
{
    /// <summary>
    /// The approved dish names sellers pick from, with their price ranges.
    /// </summary>
    /// <remarks>
    /// Cached by: <c>GetDishCatalogQuery</c>.
    /// Cleared by: <c>ApproveDishNameCommand</c>, <c>SetDishPriceRangeCommand</c>.
    /// </remarks>
    public const string DishCatalog = "dish-catalog";

    /// <summary>
    /// Dishes buyers can see: Home and the Food listing (every page, filter and sort).
    /// </summary>
    /// <remarks>
    /// Cached by: <c>SearchDishesQuery</c>.
    /// Cleared by: dish changes (<c>CreateDishCommand</c>, <c>UpdateDishCommand</c>, <c>ChangeDishPriceCommand</c>,
    /// <c>DeleteDishCommand</c>, <c>AddDishPhotoCommand</c>, <c>RemoveDishPhotoCommand</c>), shop changes that
    /// hide or show dishes (<c>BlockVendorCommand</c>, <c>UnblockVendorCommand</c>, <c>ChangeShopLogoCommand</c>,
    /// <c>DeleteAccountCommand</c>), and <c>SetDishPriceRangeCommand</c> (dishes may become unsellable).
    /// </remarks>
    public const string Dishes = "dishes";

    /// <summary>
    /// Shops buyers can see: "Meet local vendors" and Home → Top sellers.
    /// </summary>
    /// <remarks>
    /// Cached by: <c>GetVendorsQuery</c>, <c>GetTopVendorsQuery</c>.
    /// Cleared by: <c>ApproveVendorCommand</c>, <c>BlockVendorCommand</c>, <c>UnblockVendorCommand</c>,
    /// <c>UpdateShopDetailsCommand</c>, <c>ChangeShopLogoCommand</c>, <c>DeleteAccountCommand</c>,
    /// and dish commands that change a shop's dish count (<c>CreateDishCommand</c>, <c>DeleteDishCommand</c>).
    /// </remarks>
    public const string Vendors = "vendors";

    /// <summary>
    /// Delivery pricing per country: the shop fee range and the extra charge per delivery window.
    /// </summary>
    /// <remarks>
    /// Cached by: <c>GetDeliveryWindowsQuery</c> (the app's "Delivery time" picker).
    /// Cleared by: <c>SetDeliveryPricingCommand</c>, <c>SetDeliveryWindowChargeCommand</c>,
    /// <c>RemoveDeliveryWindowCommand</c>.
    /// </remarks>
    public const string DeliveryPricing = "delivery-pricing";

    /// <summary>
    /// Shipping routes customers can book, with their drop-off hubs and rates
    /// (the "Send a parcel" form's step 1).
    /// </summary>
    /// <remarks>
    /// Cached by: <c>GetShipmentRoutesQuery</c>.
    /// Cleared by: the admin route commands (create a route, set or remove a rate, update the hub,
    /// switch a route on or off), coming in Shipments Part 2.
    /// </remarks>
    public const string ShipmentRoutes = "shipment-routes";
}