using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Common.Approvals;

/// <summary>
/// Knows, for the current request, which approved change is being applied, so a handler that normally turns a
/// sensitive command into a proposal applies it instead.
/// </summary>
/// <remarks>
/// <para><b>Why not a flag on the command:</b> commands are bound from request bodies, so a flag there could be sent
/// by anyone. This lives only inside the server (scoped per request) and is set only by the approve handler, after it
/// has checked that a second person is approving.</para>
/// <para>Use: <c>using (context.Applying(change)) { await sender.Send(command); }</c>.</para>
/// </remarks>
public sealed class ApprovalContext
{
    private PendingChange? _applying;

    /// <summary>True while the approved change of this kind is being applied.</summary>
    /// <param name="kind">The kind the handler would otherwise propose.</param>
    public bool IsApplying(PendingChangeKind kind) => _applying?.Kind == kind;

    /// <summary>Marks the change as being applied until the returned object is disposed.</summary>
    /// <param name="change">The approved change.</param>
    /// <returns>Dispose to stop.</returns>
    public IDisposable Applying(PendingChange change)
    {
        _applying = change;
        return new Reset(this);
    }

    private sealed class Reset(ApprovalContext owner) : IDisposable
    {
        public void Dispose() => owner._applying = null;
    }
}
