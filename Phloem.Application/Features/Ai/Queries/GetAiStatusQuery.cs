using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Admin → AI: on or off, the model, and this calendar month's calls and spend. Not cached.</summary>
public sealed record GetAiStatusQuery : IRequest<Result<AiStatusDto>>;
