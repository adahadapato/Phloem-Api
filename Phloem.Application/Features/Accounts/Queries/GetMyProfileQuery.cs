using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

/// <summary>No parameters: "my" always means the signed-in user.</summary>
public sealed record GetMyProfileQuery : IRequest<Result<ProfileDto>>;