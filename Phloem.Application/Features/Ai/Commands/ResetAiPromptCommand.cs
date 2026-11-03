using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Commands;

/// <summary>Admin → AI: go back to Phloem's default wording (saved as a new version, so nothing is lost).</summary>
/// <param name="Key">The prompt key.</param>
/// <returns>The new version number.</returns>
public sealed record ResetAiPromptCommand(string Key) : IRequest<Result<int>>;
