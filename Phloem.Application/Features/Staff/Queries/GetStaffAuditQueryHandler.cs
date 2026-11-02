using MediatR;
using Phloem.Application.Features.Staff.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Queries;

/// <summary>Handles <see cref="GetStaffAuditQuery"/>.</summary>
public sealed class GetStaffAuditQueryHandler(IStaffAuditRepository audit, IUserProfileRepository profiles)
    : IRequestHandler<GetStaffAuditQuery, Result<PagedResult<StaffAuditEntryDto>>>
{
    private const string UnknownUser = "Unknown user";

    /// <summary>One page of the log with the actors' names (fetched once for the page).</summary>
    /// <param name="query">The filters and page.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The page.</returns>
    public async Task<Result<PagedResult<StaffAuditEntryDto>>> Handle(GetStaffAuditQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await audit.GetPagedAsync(query.ActorId, query.Action, query.Country, query.From, query.To,
                                              page, ct);

        var names = await profiles.GetFullNamesAsync(paged.Items.Select(a => a.ActorId).Distinct().ToList(), ct);
        var items = paged.Items
            .Select(a => StaffAuditEntryDto.From(a, names.GetValueOrDefault(a.ActorId, UnknownUser)))
            .ToList();

        return new PagedResult<StaffAuditEntryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
