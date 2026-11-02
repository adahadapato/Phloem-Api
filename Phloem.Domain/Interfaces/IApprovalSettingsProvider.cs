using Phloem.Domain.Models;

namespace Phloem.Domain.Interfaces;

/// <summary>Reads <see cref="ApprovalSettings"/> (implemented in Infrastructure from the "Approvals" settings).</summary>
public interface IApprovalSettingsProvider
{
    /// <summary>The current approval settings.</summary>
    ApprovalSettings Get();
}
