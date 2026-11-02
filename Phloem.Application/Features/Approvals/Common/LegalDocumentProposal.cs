using System.Text.Json;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Approvals.Common;

/// <summary>A Terms / Privacy version waiting to be published: exactly what the approver sees and approves.</summary>
/// <param name="Type">Terms or Privacy.</param>
/// <param name="Title">The document's title.</param>
/// <param name="Content">The full wording (Markdown).</param>
/// <param name="IsDraft">True while the wording is temporary.</param>
public sealed record LegalDocumentProposal(LegalDocumentType Type, string Title, string Content, bool IsDraft)
{
    /// <summary>As stored in <c>PendingChange.Payload</c>.</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Reads a stored payload; null when it isn't one.</summary>
    /// <param name="json">The payload.</param>
    public static LegalDocumentProposal? FromJson(string json)
    {
        try { return JsonSerializer.Deserialize<LegalDocumentProposal>(json); }
        catch (JsonException) { return null; }
    }
}
