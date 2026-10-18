using Phloem.Application.Features.Accounts.Commands;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Application.Features.DeliveryPricings.Commands;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Domain.Enums;
using Phloem.Presentation.Endpoints.Me;

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

        // ───────────────────────────── 3. My shop ─────────────────────────────
        [typeof(OpenShopCommand)] = new OpenShopCommand("Mama Ama's Kitchen", "hello@mamaamas.ng", Phone),
        [typeof(SetMyDeliveryFeeCommand)] = new SetMyDeliveryFeeCommand(1500m),
    };
}