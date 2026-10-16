using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Seller setup page. Blank email / phone mean "use the ones from my login account".
/// Returns the new shop's Id. The shop starts as "waiting for approval".
/// </summary>
public sealed record OpenShopCommand(string ShopName, string? BusinessEmail, string? BusinessPhone)
    : IRequest<Result<Guid>>;