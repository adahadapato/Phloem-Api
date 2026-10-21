using MediatR;
using Phloem.Application.Features.Accounts.Commands;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Application.Features.Accounts.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Users;

/// <summary>
/// Admin → Users (<c>/api/admin/users</c>, Admin role only): everyone who has signed up, and suspending people who
/// abuse Phloem.
/// </summary>
/// <remarks>
/// <para>Suspending someone: they can't sign in, they're signed out everywhere, their shop is hidden and its
/// unfinished orders cancelled (paid ones flagged for a refund), and they're emailed the reason. Their own orders as
/// a buyer carry on. It ends on its own at the chosen date, or when an admin lifts it.</para>
/// <para>A person's own details live under <c>/api/me</c>.</para>
/// </remarks>
public static class UserEndpoints
{
    /// <summary>Maps the admin user endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/users")
                       .WithTags("Admin: Users")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        admin.MapGet("/", async ([AsParameters] GetAdminUsersQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetAdminUsers")
            .WithSummary("Everyone who has signed up, newest first: search by name, or only suspended people")
            .Produces<PagedResult<AdminUserDto>>()
            .ProducesValidationProblem();

        admin.MapPost("/{userId:guid}/suspend",
                async (Guid userId, SuspendRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new SuspendUserCommand(userId, body.Reason, body.UntilUtc), ct)).ToHttp())
            .WithName("SuspendUser")
            .WithSummary("Suspend someone until a date (UntilUtc) or until further notice (UntilUtc = null)")
            .WithDescription("Signs them out everywhere, hides their shop and cancels its unfinished orders, " +
                             "emails them the reason. Admins can't be suspended.")
            .Produces<UserSuspendedDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPost("/{userId:guid}/reinstate", async (Guid userId, ISender sender, CancellationToken ct) =>
                (await sender.Send(new ReinstateUserCommand(userId), ct)).ToHttp())
            .WithName("ReinstateUser")
            .WithSummary("Lift a suspension: they can sign in again and their shop comes back")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }

    /// <summary>Body of "suspend" (the person comes from the URL). <see cref="UntilUtc"/> null = until further notice.</summary>
    public sealed record SuspendRequest(string Reason, DateTime? UntilUtc);
}
