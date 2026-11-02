using MediatR;
using Phloem.Application.Features.Staff.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Queries;

/// <summary>Admin → Staff → Job roles: every job role with its permissions and member count.</summary>
/// <remarks>Needs <c>staff.manage</c> to view; only admins can change a role's permissions.</remarks>
public sealed record GetJobRolesQuery : IRequest<Result<JobRolesDto>>;
