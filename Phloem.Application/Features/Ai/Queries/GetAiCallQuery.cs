using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>One AI call with its text. It can hold people's text, so every open is written to the audit log.</summary>
/// <param name="Id">The call.</param>
public sealed record GetAiCallQuery(Guid Id) : IRequest<Result<AiCallDetailsDto>>;
