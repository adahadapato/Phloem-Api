using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Application.Features.Orders.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authorization;

namespace Phloem.Presentation.Endpoints.Orders;

/// <summary>
/// Admin → Orders (<c>/api/admin/orders</c>, permission <c>orders.view</c>): read-only.
/// </summary>
/// <remarks>
/// <para>Only orders delivered to the person's countries are listed; any other order answers 404. Opening an order is
/// written to the audit log (staff see the customer's address and phone).</para>
/// </remarks>
public static class AdminOrderEndpoints
{
    /// <summary>Maps the admin order endpoints.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapAdminOrderEndpoints(this IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/orders")
                       .WithTags("Admin: Orders")
                       .RequirePermission(Permissions.Orders.View, CountryRule.RecordCountry);

        admin.MapGet("/", async ([AsParameters] GetAdminOrdersQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetAdminOrders")
            .WithSummary("Orders in your countries, newest first; filter by country, shop, status, dates, search")
            .Produces<PagedResult<AdminOrderSummaryDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        admin.MapGet("/{orderNumber}", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminOrderQuery(orderNumber), ct)).ToHttp())
            .WithName("GetAdminOrder")
            .WithSummary("One order with its timeline (full names); the view is recorded in the audit log")
            .Produces<AdminOrderDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }
}
