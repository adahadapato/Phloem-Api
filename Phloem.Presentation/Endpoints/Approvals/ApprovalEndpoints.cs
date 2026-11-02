using MediatR;
using Phloem.Application.Features.Approvals.Commands;
using Phloem.Application.Features.Approvals.Dtos;
using Phloem.Application.Features.Approvals.Queries;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Approvals;

/// <summary>
/// Admin → Approvals (<c>/api/admin/approvals</c>): changes that need a second person.
/// </summary>
/// <remarks>
/// <para>Open to signed-in staff; the handlers decide who may see and decide what (owners decide everything;
/// Country managers confirm shop blocks in their countries; nobody decides their own).</para>
/// <para>Needing approval so far: publishing Terms / Privacy, and confirming a shop block (unblocked automatically
/// if nobody confirms within 24 hours).</para>
/// </remarks>
public static class ApprovalEndpoints
{
    /// <summary>Maps the approval endpoints.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapApprovalEndpoints(this IEndpointRouteBuilder api)
    {
        var approvals = api.MapGroup("/admin/approvals")
                           .WithTags("Admin: Approvals")
                           .RequireAuthorization();

        approvals.MapGet("/", async ([AsParameters] GetApprovalsQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetApprovals")
            .WithSummary("Waiting for you, your proposals, or history (view = Waiting | Mine | History)")
            .Produces<PagedResult<PendingChangeDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        approvals.MapGet("/count", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetApprovalCountQuery(), ct)).ToHttp())
            .WithName("GetApprovalCount")
            .WithSummary("How many changes are waiting for you")
            .Produces<int>();

        approvals.MapPost("/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken ct) =>
                (await sender.Send(new ApproveChangeCommand(id), ct)).ToHttp())
            .WithName("ApproveChange")
            .WithSummary("Approve someone else's change: it's applied now; returns what happened")
            .Produces<string>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        approvals.MapPost("/{id:guid}/reject", async (Guid id, RejectRequest body, ISender sender,
                                                      CancellationToken ct) =>
                (await sender.Send(new RejectChangeCommand(id, body.Reason), ct)).ToHttp())
            .WithName("RejectChange")
            .WithSummary("Reject someone else's change, with a reason (a rejected block unblocks the shop)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        approvals.MapPost("/{id:guid}/withdraw", async (Guid id, ISender sender, CancellationToken ct) =>
                (await sender.Send(new WithdrawChangeCommand(id), ct)).ToHttp())
            .WithName("WithdrawChange")
            .WithSummary("Take back your own waiting Terms / Privacy proposal")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return api;
    }

    /// <summary>Body of "reject".</summary>
    /// <param name="Reason">Why (shown to the person who proposed it).</param>
    public sealed record RejectRequest(string Reason);
}
