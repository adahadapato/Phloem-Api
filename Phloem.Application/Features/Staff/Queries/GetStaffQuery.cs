using MediatR;
using Phloem.Application.Features.Staff.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Queries;

/// <summary>
/// Admin → Staff: the owners (Admin) and everyone with a staff job role, with their roles, countries and whether
/// two-factor sign-in is on.
/// </summary>
/// <remarks>Needs <c>staff.manage</c>. Someone who covers only some countries sees the owners plus staff whose home
/// country or covered countries overlap theirs.</remarks>
public sealed record GetStaffQuery : IRequest<Result<IReadOnlyList<StaffMemberDto>>>;
