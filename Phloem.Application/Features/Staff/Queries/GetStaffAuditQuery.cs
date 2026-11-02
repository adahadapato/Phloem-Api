using MediatR;
using Phloem.Application.Features.Staff.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Queries;

/// <summary>Admin → Audit log: who did what, newest first, with optional filters.</summary>
/// <param name="ActorId">Only what this person did.</param>
/// <param name="Action">Only this kind of action.</param>
/// <param name="Country">Only this country.</param>
/// <param name="From">Only from this moment (UTC).</param>
/// <param name="To">Only before this moment (UTC).</param>
/// <param name="Page">Page number, from 1.</param>
/// <param name="PageSize">Lines per page.</param>
/// <remarks>Needs <c>audit.view</c> and every country.</remarks>
public sealed record GetStaffAuditQuery(
    Guid? ActorId = null,
    StaffAction? Action = null,
    Country? Country = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IRequest<Result<PagedResult<StaffAuditEntryDto>>>;
