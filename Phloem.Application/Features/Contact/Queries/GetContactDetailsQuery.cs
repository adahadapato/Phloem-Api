using MediatR;
using Phloem.Application.Features.Contact.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Contact.Queries;

/// <summary>Contact us page: the support email, phone, WhatsApp, address, hours and reply time (from settings).</summary>
public sealed record GetContactDetailsQuery : IRequest<Result<ContactDetailsDto>>;

/// <summary>Handles <see cref="GetContactDetailsQuery"/>. Settings only: no database call, so not cached.</summary>
public sealed class GetContactDetailsQueryHandler(IContactDetailsProvider contact)
    : IRequestHandler<GetContactDetailsQuery, Result<ContactDetailsDto>>
{
    public Task<Result<ContactDetailsDto>> Handle(GetContactDetailsQuery query, CancellationToken ct) =>
        Task.FromResult(Result.Success(ContactDetailsDto.From(contact.Get())));
}
