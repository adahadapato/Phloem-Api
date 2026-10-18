using MediatR;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Auth;

/// <summary>
/// <c>/api/auth</c>: getting in and out. Every endpoint is open (no token needed); each one sends its command through
/// MediatR and returns the <c>Result</c> as HTTP.
/// </summary>
/// <remarks>
/// <para>Login, register, 2FA and refresh all return <see cref="AuthTokensDto"/>: either signed in (tokens) or
/// <c>requiresTwoFactor</c> with a challenge token for <c>/login/two-factor</c>.</para>
/// <para>Logout, forgot-password and reset-password return 204. Forgot-password ALWAYS returns 204, so it can't be used
/// to find out which emails have accounts.</para>
/// </remarks>
public static class AuthEndpoints
{
    /// <summary>Maps the auth endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder api)
    {
        var auth = api.MapGroup("/auth")
                      .WithTags("Auth")
                      .AllowAnonymous();

        // 1. Sign-up
        auth.MapPost("/register", async (RegisterCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("Register")
            .WithSummary("Create an account and sign in")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        // 2. Sign-in
        auth.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("Login")
            .WithSummary("Sign in with email and password (may ask for a 2FA code)")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        auth.MapPost("/login/two-factor", async (CompleteTwoFactorLoginCommand command, ISender sender,
                                                CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("CompleteTwoFactorLogin")
            .WithSummary("Finish sign-in with an authenticator code or a recovery code")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // 3. Sessions
        auth.MapPost("/refresh", async (RefreshTokenCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("RefreshToken")
            .WithSummary("Swap a refresh token for a new pair (stay signed in)")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem();

        auth.MapPost("/logout", async (RevokeTokenCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("Logout")
            .WithSummary("Sign out: the refresh token stops working")
            .Produces(StatusCodes.Status204NoContent);

        // 4. Password reset
        auth.MapPost("/forgot-password", async (ForgotPasswordCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("ForgotPassword")
            .WithSummary("Email a password-reset link (always succeeds)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        auth.MapPost("/reset-password", async (ResetPasswordCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("ResetPassword")
            .WithSummary("Choose a new password using the emailed link")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        return api;
    }
}