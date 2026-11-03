using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Commands;

/// <summary>Admin → AI: save a new wording for a prompt (one step, logged). It's used at once.</summary>
/// <param name="Key">The prompt key.</param>
/// <param name="Text">The new wording (must still ask for every required answer field).</param>
/// <param name="Note">Why it changed (optional, shown in the version history).</param>
/// <returns>The new version number.</returns>
public sealed record SaveAiPromptCommand(string Key, string Text, string? Note) : IRequest<Result<int>>;
