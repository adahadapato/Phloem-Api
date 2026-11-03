using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Commands;

/// <summary>Admin → AI: bring back an older wording (saved as a new version, so nothing is lost).</summary>
/// <param name="Key">The prompt key.</param>
/// <param name="Version">The version to bring back.</param>
/// <returns>The new version number.</returns>
public sealed record RestoreAiPromptVersionCommand(string Key, int Version) : IRequest<Result<int>>;
