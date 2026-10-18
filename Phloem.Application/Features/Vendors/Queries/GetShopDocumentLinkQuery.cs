using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>Admin → open a document: a short-lived signed link to the private file.</summary>
public sealed record GetShopDocumentLinkQuery(Guid ShopId, Guid DocumentId) : IRequest<Result<string>>;
