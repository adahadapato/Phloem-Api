using System.Security.Claims;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Infrastructure.Settings.Identity;

namespace Phloem.Presentation.Services;

/// <summary>
/// Who is making the current request, read from the access token that JWT authentication has already checked.
/// Handlers use this instead of a user Id in the request body, so nobody can act as someone else.
/// </summary>
/// <remarks>
/// <para>No token, or an invalid one: <see cref="UserId"/> is null and <see cref="IsAuthenticated"/> is false.</para>
/// <para>Roles, staff permissions ("perm") and countries ("cty") come from the token, so a change takes effect at the
/// next token refresh (at most <c>Jwt:ExpiryMinutes</c> later).</para>
/// <para>A country name in the token that Phloem doesn't know (e.g. one removed later) is ignored.</para>
/// </remarks>
internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    /// <summary>The checked token's claims for this request; null outside a request.</summary>
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    /// <inheritdoc />
    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(JwtClaimNames.UserId), out var id) ? id : null;

    /// <inheritdoc />
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true && UserId is not null;

    /// <inheritdoc />
    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;

    /// <inheritdoc />
    public bool HasPermission(string permission) =>
        Principal is { } user
        && (user.HasClaim(JwtClaimNames.Permission, JwtClaimNames.All)
            || user.HasClaim(JwtClaimNames.Permission, permission));

    /// <inheritdoc />
    public bool CoversAllCountries => Principal?.HasClaim(JwtClaimNames.Country, JwtClaimNames.All) == true;

    /// <inheritdoc />
    public IReadOnlyCollection<Country> Countries =>
        Principal is null || CoversAllCountries
            ? Array.Empty<Country>()
            : Principal.FindAll(JwtClaimNames.Country)
                       .Select(c => Enum.TryParse<Country>(c.Value, out var country) && Enum.IsDefined(country)
                                        ? country
                                        : (Country?)null)
                       .OfType<Country>()
                       .Distinct()
                       .ToList();

    /// <inheritdoc />
    public bool CoversCountry(Country country) => CoversAllCountries || Countries.Contains(country);
}
