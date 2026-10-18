using Phloem.Application.Features.Accounts.Commands;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Application.Features.DeliveryPricings.Commands;
using Phloem.Application.Features.DishCatalogs.Commands;
using Phloem.Application.Features.Dishes.Commands;
using Phloem.Application.Features.Orders.Commands;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Application.Features.Orders.Queries;
using Phloem.Application.Features.ShipmentRoutes.Commands;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Domain.Enums;
using Phloem.Presentation.Endpoints.Commissions;
using Phloem.Presentation.Endpoints.DeliveryPricings;
using Phloem.Presentation.Endpoints.DishCatalogs;
using Phloem.Presentation.Endpoints.Dishes;
using Phloem.Presentation.Endpoints.Me;
using Phloem.Presentation.Endpoints.ShipmentRoutes;
using Phloem.Presentation.Endpoints.Shops;

namespace Phloem.Presentation.Settings.OpenApi;

/// <summary>
/// Realistic request bodies shown in Swagger in place of <c>"string"</c>, one per request type.
/// </summary>
/// <remarks>
/// Built from the real types, so renaming or adding a field breaks the build here instead of leaving Swagger
/// out of date. Add a line when you add an endpoint that takes a body.
/// </remarks>
internal static class RequestExamples
{
    // ───────────────────────────── Shared values ─────────────────────────────

    /// <summary>A home address used by sign-up and the personal-details page.</summary>
    private static readonly HomeAddressInput HomeAddress =
        new("12 Allen Avenue", null, "Ikeja", "Lagos", Country.Nigeria);

    /// <summary>Stands in for a dish name's Id: replace it with one from GET /api/dish-catalog.</summary>
    private static readonly Guid DishCatalogId = Guid.Parse("0199f0a1-7c2e-7a40-9b1d-5e3f2c8a6d10");

    /// <summary>Stands in for a dish's Id: replace it with one from GET /api/dishes.</summary>
    private static readonly Guid DishId = Guid.Parse("0199f0b2-1a3c-7d55-8e2f-4b6a9c0d1e22");

    /// <summary>Stands in for a saved address's Id: replace it with one from GET /api/me.</summary>
    private static readonly Guid DeliveryAddressId = Guid.Parse("0199f0b2-5d7e-7f10-a3b4-6c8d0e2f4a35");

    /// <summary>A basket: two of one dish.</summary>
    private static readonly IReadOnlyList<BasketItemDto> Basket = [new BasketItemDto(DishId, 2)];

    private const string Email = "ama.mensah@example.com";
    private const string Password = "Str0ng!Passw0rd";
    private const string NewPassword = "N3w!Passw0rd";
    private const string Phone = "+2348023176049";
    private const string RefreshToken = "paste the refreshToken from login";

    /// <summary>Example body for each request type.</summary>
    public static readonly IReadOnlyDictionary<Type, object> ByType = new Dictionary<Type, object>
    {
        // ───────────────────────────── 1. Auth ─────────────────────────────
        [typeof(RegisterCommand)] = new RegisterCommand(
            "Ama", "Mensah", Email, Phone, Country.Nigeria,
            new DateOnly(1990, 5, 14), HomeAddress, Password, AgreedToTerms: true),
        [typeof(LoginCommand)] = new LoginCommand(Email, Password),
        [typeof(CompleteTwoFactorLoginCommand)] = new CompleteTwoFactorLoginCommand(
            "paste the challengeToken from login", "123456", IsRecoveryCode: false),
        [typeof(RefreshTokenCommand)] = new RefreshTokenCommand(RefreshToken),
        [typeof(RevokeTokenCommand)] = new RevokeTokenCommand(RefreshToken),
        [typeof(ForgotPasswordCommand)] = new ForgotPasswordCommand(Email),
        [typeof(ResetPasswordCommand)] = new ResetPasswordCommand(
            Email, "paste the token from the email link", NewPassword, NewPassword),
        [typeof(ConfirmEmailCommand)] = new ConfirmEmailCommand(
            Guid.Parse("3f2a9c4e-8b1d-4e6f-9a7c-2d5b8e1f4a60"), "paste the token from the email link"),

        // ───────────────────────────── 2. Me ─────────────────────────────
        [typeof(UpdateProfileCommand)] = new UpdateProfileCommand("Ama", "Mensah", Country.Nigeria, Phone),
        [typeof(UpdateNotificationsCommand)] = new UpdateNotificationsCommand(
            OrderUpdates: true, ShipmentUpdates: true, Offers: false, NewDishes: true, ShopOrders: true),
        [typeof(UpdatePersonalDetailsCommand)] = new UpdatePersonalDetailsCommand(
            new DateOnly(1990, 5, 14), HomeAddress),
        [typeof(AddAddressCommand)] = new AddAddressCommand(
            "Work", "5 Broad Street, Lagos Island, Lagos", Country.Nigeria, MakeDefault: false),
        [typeof(MeEndpoints.AddressRequest)] = new MeEndpoints.AddressRequest(
            "Work", "5 Broad Street, Lagos Island, Lagos", Country.Nigeria),
        [typeof(ChangePasswordCommand)] = new ChangePasswordCommand(Password, NewPassword, NewPassword),
        [typeof(EnableTwoFactorCommand)] = new EnableTwoFactorCommand("123456"),
        [typeof(DisableTwoFactorCommand)] = new DisableTwoFactorCommand(Password),
        [typeof(RegenerateRecoveryCodesCommand)] = new RegenerateRecoveryCodesCommand(Password),

        // ───────────────────────────── 3. Shops ─────────────────────────────
        [typeof(OpenShopCommand)] = new OpenShopCommand("Mama Ama's Kitchen", "hello@mamaamas.ng", Phone),
        [typeof(UpdateShopDetailsCommand)] = new UpdateShopDetailsCommand(
            "Mama Ama's Kitchen", "hello@mamaamas.ng", Phone),
        [typeof(SetMyDeliveryFeeCommand)] = new SetMyDeliveryFeeCommand(1500m),
        [typeof(ShopEndpoints.ReasonRequest)] = new ShopEndpoints.ReasonRequest(
            "The business email doesn't match the registration certificate. Please correct it."),
        [typeof(ShopEndpoints.ReviewRequest)] = new ShopEndpoints.ReviewRequest(
            Accept: false, "The photo is blurred; please upload a clearer copy."),
        [typeof(ShopEndpoints.VerifiedRequest)] = new ShopEndpoints.VerifiedRequest(Verified: true),
        [typeof(ShopEndpoints.BlockedRequest)] = new ShopEndpoints.BlockedRequest(Blocked: true),

        // ───────────────────────────── 4. My dishes ─────────────────────────────
        [typeof(CreateDishCommand)] = new CreateDishCommand(
            DishCatalogId, "with fried plantain", 4500m, "Smoky party jollof with grilled chicken.", IsAvailable: true),
        [typeof(DishEndpoints.DishRequest)] = new DishEndpoints.DishRequest(
            DishCatalogId, "with fried plantain", 4500m, "Smoky party jollof with grilled chicken.", IsAvailable: true),
        [typeof(DishEndpoints.PriceRequest)] = new DishEndpoints.PriceRequest(5000m),

        // ───────────────────────────── 5. Dish catalog ─────────────────────────────
        [typeof(SuggestDishNameCommand)] = new SuggestDishNameCommand(
            "Ofada Rice & Ayamase", DishCategory.LocalMeals, CatalogGroup.Nigeria),
        [typeof(DishCatalogEndpoints.PriceRangeRequest)] = new DishCatalogEndpoints.PriceRangeRequest(3000m, 6000m),

        // ───────────────────────────── 6. Pricing and routes (admin) ─────────────────────────────
        [typeof(CommissionEndpoints.CommissionRateRequest)] = new CommissionEndpoints.CommissionRateRequest(10m),
        [typeof(DeliveryPricingEndpoints.ShopFeeRangeRequest)] =
            new DeliveryPricingEndpoints.ShopFeeRangeRequest(MinShopFee: 800m, MaxShopFee: 3500m),
        [typeof(DeliveryPricingEndpoints.WindowChargeRequest)] =
            new DeliveryPricingEndpoints.WindowChargeRequest(ExtraCharge: 500m),
        [typeof(CreateShipmentRouteCommand)] = new CreateShipmentRouteCommand(
            Country.UnitedKingdom, Country.Nigeria,
            "Unit 4, Phloem Hub, 12 Commercial Road, London E1 1AA", "Mon–Sat, 9am–6pm"),
        [typeof(ShipmentRouteEndpoints.DropOffRequest)] = new ShipmentRouteEndpoints.DropOffRequest(
            "Unit 4, Phloem Hub, 12 Commercial Road, London E1 1AA", "Mon–Sat, 9am–6pm"),
        [typeof(ShipmentRouteEndpoints.ActiveRequest)] = new ShipmentRouteEndpoints.ActiveRequest(IsActive: true),
        [typeof(ShipmentRouteEndpoints.ShipmentRateRequest)] = new ShipmentRouteEndpoints.ShipmentRateRequest(
            BaseCharge: 15m, PricePerKg: 6.5m, MinTransitDays: 5, MaxTransitDays: 10),

        // ───────────────────────────── 7. Orders ─────────────────────────────
        [typeof(CheckBasketQuery)] = new CheckBasketQuery(Basket, DeliveryWindow.Asap),
        [typeof(PlaceOrderCommand)] = new PlaceOrderCommand(Basket, DeliveryAddressId, DeliveryWindow.Asap),
    };
}
