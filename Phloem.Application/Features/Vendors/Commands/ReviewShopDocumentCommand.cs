using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Admin accepts or rejects one document (a reason is required to reject).</summary>
public sealed record ReviewShopDocumentCommand(Guid ShopId, Guid DocumentId, bool Accept, string? Reason)
    : IRequest<Result>;
