using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Admin → AI: every AI prompt, with the version in use and who saved it. Not cached.</summary>
public sealed record GetAiPromptsQuery : IRequest<Result<IReadOnlyList<AiPromptSummaryDto>>>;
