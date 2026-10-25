using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Admin → Users: gives a person a role, or takes it away. Only <c>Roles.Assignable</c> (Admin, CatalogManager,
/// ShipmentManager, SupportAgent). Repeat-safe: giving a role they have, or taking one they don't, succeeds.
/// </summary>
/// <remarks>
/// <para>Guards: an admin can't take Admin from themselves, the last admin keeps Admin, and a suspended person can't
/// be made an admin.</para>
/// <para>Taking a role away signs the person out everywhere, so it takes effect straight away (roles travel in the
/// access token). A new role arrives at their next sign-in or token refresh.</para>
/// </remarks>
/// <param name="UserId">The person.</param>
/// <param name="Role">One of <c>Roles.Assignable</c>, e.g. "Admin".</param>
/// <param name="Grant">True = give it; false = take it away.</param>
public sealed record SetUserRoleCommand(Guid UserId, string Role, bool Grant) : IRequest<Result>;
