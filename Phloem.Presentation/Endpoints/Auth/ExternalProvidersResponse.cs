namespace Phloem.Presentation.Endpoints.Auth;

/// <summary>
/// Answer of <c>GET /api/auth/external/providers</c>: which outside sign-ins are switched on, so the app shows a
/// "Continue with …" button only when it will work.
/// </summary>
/// <remarks>Lives in Presentation, not Application: it only reports the API's own settings (whether the Google keys
/// are filled in), not anything from the business rules.</remarks>
/// <param name="Google">True when "Continue with Google" works (both Google keys are set).</param>
public sealed record ExternalProvidersResponse(bool Google);
