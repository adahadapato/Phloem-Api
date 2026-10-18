using Phloem.Presentation.Endpoints.Auth;
using Phloem.Presentation.Endpoints.Commissions;
using Phloem.Presentation.Endpoints.DeliveryPricings;
using Phloem.Presentation.Endpoints.DishCatalogs;
using Phloem.Presentation.Endpoints.Dishes;
using Phloem.Presentation.Endpoints.Me;
using Phloem.Presentation.Endpoints.ShipmentRoutes;
using Phloem.Presentation.Endpoints.Shops;

namespace Phloem.Presentation.Extensions;

/// <summary>
/// Maps every endpoint, so Program.cs has one line. Feature groups live under <c>/api</c> (each feature file holds its
/// user AND admin endpoints); <c>/health</c> and signed private-file links sit outside it.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>Maps all endpoints.</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication MapPhloemEndpoints(this WebApplication app)
    {
        // 1. API groups
        var api = app.MapGroup("/api");
        api.MapAuthEndpoints();              // /api/auth
        api.MapMeEndpoints();                // /api/me
        api.MapShopEndpoints();              // /api/shops            + /api/admin/shops
        api.MapDishEndpoints();              // /api/shops/mine/dishes
        api.MapDishCatalogEndpoints();       // /api/dish-catalog     + /api/admin/dish-catalog
        api.MapCommissionEndpoints();        //                         /api/admin/commissions
        api.MapDeliveryPricingEndpoints();   // /api/delivery-windows + /api/admin/delivery-pricing
        api.MapShipmentRouteEndpoints();     //                         /api/admin/shipment-routes
                                             // next: /api/dishes (buyers), /api/orders, /api/shipments

        // 2. Private documents through signed links, e.g. /private-files/…?expires=…&sig=… (not in Swagger)
        app.MapPrivateFiles();

        // 3. Health (not in Swagger)
        app.MapGet("/health",
                () => Results.Ok(
                    new
                    {
                        status = "healthy",
                        timestamp = DateTime.UtcNow
                    }))
            .ExcludeFromDescription();

        return app;
    }
}
