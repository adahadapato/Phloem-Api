using MediatR;
using Phloem.Application.Features.Contact.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Contact.Queries;

/// <summary>Admin → Contact messages: one page of the inbox, newest first, optionally one status. Never cached.</summary>
/// <param name="Status">Only this status; null = all.</param>
/// <param name="Page">1-based.</param>
/// <param name="PageSize">1–100.</param>
public sealed record GetContactMessagesQuery(
    ContactMessageStatus? Status = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IRequest<Result<ContactInboxDto>>;

/// <summary>Handles <see cref="GetContactMessagesQuery"/>.</summary>
public sealed class GetContactMessagesQueryHandler(IContactMessageRepository messages)
    : IRequestHandler<GetContactMessagesQuery, Result<ContactInboxDto>>
{
    public async Task<Result<ContactInboxDto>> Handle(GetContactMessagesQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await messages.GetPageAsync(query.Status, page, ct);
        var counts = await messages.CountByStatusAsync(ct);

        var items = paged.Items.Select(ContactMessageDto.From).ToList();
        return new ContactInboxDto(
            new PagedResult<ContactMessageDto>(items, paged.TotalCount, paged.Page, paged.PageSize),
            counts.GetValueOrDefault(ContactMessageStatus.New),
            counts.GetValueOrDefault(ContactMessageStatus.Replied),
            counts.GetValueOrDefault(ContactMessageStatus.Closed));
    }
}
