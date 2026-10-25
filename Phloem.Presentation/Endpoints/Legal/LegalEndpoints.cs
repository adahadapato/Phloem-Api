using MediatR;
using Phloem.Application.Features.LegalDocuments.Commands;
using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Application.Features.LegalDocuments.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Legal;

/// <summary>
/// The Terms &amp; Conditions and the Privacy Policy, kept in the database so the website and the app show the same
/// text without either having it built in.
/// <list type="number">
///   <item><description><b>Public</b> (<c>/api/legal/{type}</c>): the current version. No sign-in (read before
///   signing up).</description></item>
///   <item><description><b>Admin</b> (<c>/api/admin/legal/{type}</c>): publish a new version. Admin role only.
///   </description></item>
/// </list>
/// </summary>
public static class LegalEndpoints
{
    /// <summary>Maps the legal endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapLegalEndpoints(this IEndpointRouteBuilder api)
    {
        MapPublic(api);
        MapAdmin(api);
        return api;
    }

    // ═════════════════════════════ A. Public ═════════════════════════════

    private static void MapPublic(IEndpointRouteBuilder api)
    {
        var legal = api.MapGroup("/legal")
                       .WithTags("Legal");

        legal.MapGet("/{type}", async (LegalDocumentType type, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetLegalDocumentQuery(type), ct)).ToHttp())
            .WithName("GetLegalDocument")
            .WithSummary("The current Terms (type=Terms) or Privacy Policy (type=Privacy), in Markdown")
            .WithDescription("isDraft = true while the wording is temporary (show a \"Draft\" note). Headings may " +
                             "carry anchors like {#sellers} and {#parcels} so links can open a section.")
            .Produces<LegalDocumentDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ═════════════════════════════ B. Admin ═════════════════════════════

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/legal")
                       .WithTags("Admin: Legal")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        admin.MapPost("/{type}", async (LegalDocumentType type, LegalDocumentRequest body, ISender sender,
                                        CancellationToken ct) =>
                (await sender.Send(new PublishLegalDocumentCommand(type, body.Title, body.Content, body.IsDraft), ct))
                    .ToHttp())
            .WithName("PublishLegalDocument")
            .WithSummary("Publish a new version (the old ones are kept); returns the new version number")
            .Produces<int>()
            .ProducesValidationProblem();
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "publish" (the type comes from the URL).</summary>
    /// <param name="Title">e.g. "Terms &amp; Conditions".</param>
    /// <param name="Content">The full text, in Markdown.</param>
    /// <param name="IsDraft">True if the wording is still temporary.</param>
    public sealed record LegalDocumentRequest(string Title, string Content, bool IsDraft);
}
