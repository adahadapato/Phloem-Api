using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;

namespace Phloem.Infrastructure.Settings.Approvals;

/// <summary>Implements <see cref="IApprovalSettingsProvider"/> from <see cref="ApprovalSettingsOptions"/>.</summary>
internal sealed class ApprovalSettingsProvider(IOptionsMonitor<ApprovalSettingsOptions> options) : IApprovalSettingsProvider
{
    /// <inheritdoc />
    public ApprovalSettings Get()
    {
        var o = options.CurrentValue;
        return new ApprovalSettings(TimeSpan.FromDays(o.ProposalDays), TimeSpan.FromHours(o.BlockConfirmHours));
    }
}
