using MediatR;
using Phloem.Application.Features.ParcelItemRules.Commands;
using Phloem.Application.Features.ParcelItemRules.Dtos;
using Phloem.Application.Features.ParcelItemRules.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.ParcelItemRules;

/// <summary>
/// The "what can be sent" list shown when booking a parcel, kept in the database so it can change without an app
/// update.
/// <list type="number">
///   <item><description><b>Public</b> (<c>/api/parcel-rules</c>): the list, in display order. No sign-in.
///   </description></item>
///   <item><description><b>Admin</b> (<c>/api/admin/parcel-rules</c>): add, change, delete and reorder lines.
///   Admin role only.</description></item>
/// </list>
/// </summary>
public static class ParcelItemRuleEndpoints
{
    /// <summary>Maps the parcel rule endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapParcelItemRuleEndpoints(this IEndpointRouteBuilder api)
    {
        MapPublic(api);
        MapAdmin(api);
        return api;
    }

    // ═════════════════════════════ A. Public ═════════════════════════════

    private static void MapPublic(IEndpointRouteBuilder api)
    {
        api.MapGet("/parcel-rules", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetParcelItemRulesQuery(), ct)).ToHttp())
            .WithTags("Shipping")
            .WithName("GetParcelItemRules")
            .WithSummary("What parcels may not contain (kind=NotAllowed) or only with a condition (kind=Restricted)")
            .Produces<IReadOnlyList<ParcelItemRuleDto>>();
    }

    // ═════════════════════════════ B. Admin ═════════════════════════════

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/parcel-rules")
                       .WithTags("Admin: What can be sent")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        admin.MapPost("/", async (ParcelItemRuleRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new AddParcelItemRuleCommand(body.Kind, body.Name, body.Detail), ct)).ToHttp())
            .WithName("AddParcelItemRule")
            .WithSummary("Add a line at the end of the list; returns its id")
            .Produces<Guid>()
            .ProducesValidationProblem();

        admin.MapPut("/order", async (ReorderRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new ReorderParcelItemRulesCommand(body.Ids), ct)).ToHttp())
            .WithName("ReorderParcelItemRules")
            .WithSummary("Put the list in a new order (every line's id, first to last)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPut("/{id:guid}", async (Guid id, ParcelItemRuleRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new UpdateParcelItemRuleCommand(id, body.Kind, body.Name, body.Detail), ct))
                    .ToHttp())
            .WithName("UpdateParcelItemRule")
            .WithSummary("Change a line's kind, name or detail")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                (await sender.Send(new DeleteParcelItemRuleCommand(id), ct)).ToHttp())
            .WithName("DeleteParcelItemRule")
            .WithSummary("Remove a line from the list")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "add" and "change" (the id, for a change, comes from the URL).</summary>
    /// <param name="Kind">NotAllowed or Restricted.</param>
    /// <param name="Name">e.g. "Fresh meat or fish".</param>
    /// <param name="Detail">Optional condition or reason.</param>
    public sealed record ParcelItemRuleRequest(ParcelItemRuleKind Kind, string Name, string? Detail);

    /// <summary>Body of "reorder".</summary>
    /// <param name="Ids">Every line's id, first to last.</param>
    public sealed record ReorderRequest(IReadOnlyList<Guid> Ids);
}
