using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

/// <summary>Admin → Users: everyone who has signed up, newest first. Not cached.</summary>
/// <param name="Search">Part of the name; null = everyone.</param>
/// <param name="SuspendedOnly">True = only people who are suspended.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">People per page.</param>
public sealed record GetAdminUsersQuery(
    string? Search,
    bool SuspendedOnly = false,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IRequest<Result<PagedResult<AdminUserDto>>>;
