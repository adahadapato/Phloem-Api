using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Seller setup page. Blank email / phone mean "use the ones from my login account".
/// Returns the new shop's Id. The shop starts as "waiting for approval".
/// </summary>
/// <param name="AgreedToSellerTerms">"I agree to the Seller terms" ticked (required; recorded as proof).</param>
public sealed record OpenShopCommand(string ShopName, string? BusinessEmail, string? BusinessPhone,
                                     bool AgreedToSellerTerms = false)
    : IRequest<Result<Guid>>;