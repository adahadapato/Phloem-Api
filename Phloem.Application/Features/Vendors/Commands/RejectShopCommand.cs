using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Admin rejects a shop waiting for approval, with a reason the seller sees and can fix.</summary>
public sealed record RejectShopCommand(Guid ShopId, string Reason) : IRequest<Result>;
