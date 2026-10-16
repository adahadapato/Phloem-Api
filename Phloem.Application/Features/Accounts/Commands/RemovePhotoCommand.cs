using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Edit profile → Remove photo.</summary>
public sealed record RemovePhotoCommand : IRequest<Result>;