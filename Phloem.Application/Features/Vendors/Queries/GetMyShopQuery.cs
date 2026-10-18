using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>The signed-in seller's own shop (any status, including waiting for approval or rejected).</summary>
public sealed record GetMyShopQuery : IRequest<Result<MyShopDto>>;