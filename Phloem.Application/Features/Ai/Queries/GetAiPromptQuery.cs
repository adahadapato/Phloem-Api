using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>One AI prompt for editing, with its saved versions. Not cached.</summary>
/// <param name="Key">The prompt key, e.g. "prompt.rewrite".</param>
public sealed record GetAiPromptQuery(string Key) : IRequest<Result<AiPromptDetailsDto>>;
