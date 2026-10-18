using MediatR;
using Phloem.Application.Features.Commissions.Commands;
using Phloem.Application.Features.Commissions.Dtos;
using Phloem.Application.Features.Commissions.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Commissions;

/// <summary>
/// Commissions: the share Phloem keeps from each delivered order, one rate per country.
/// <list type="number">
///   <item><description><b>Admin</b> (<c>/api/admin/commissions</c>): view and set the rates. Admin role only.</description></item>
/// </list>
/// </summary>
/// <remarks>Sellers see the effect on their payouts with the orders batch; there is no seller endpoint here yet.</remarks>
public static class CommissionEndpoints
{
    /// <summary>Maps the commission endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapCommissionEndpoints(this IEndpointRouteBuilder api)
    {
        MapAdmin(api);
        return api;
    }

    // ═════════════════════════════ A. Admin ═════════════════════════════

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/commissions")
                       .WithTags("Admin: Commissions")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminCommissionsQuery(), ct)).ToHttp())
            .WithName("GetAdminCommissions")
            .WithSummary("The commission rate in every country (isSet = false: not set yet)")
            .Produces<IReadOnlyList<CommissionDto>>();

        admin.MapPut("/{country}", async (Country country, CommissionRateRequest body, ISender sender,
                                          CancellationToken ct) =>
                (await sender.Send(new SetCommissionCommand(country, body.RatePercent), ct)).ToHttp())
            .WithName("SetCommission")
            .WithSummary("Set a country's commission rate (only orders delivered from now on use it)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "set a commission" (the country comes from the URL), e.g. 10 = 10% of the dishes.</summary>
    public sealed record CommissionRateRequest(decimal RatePercent);
}
