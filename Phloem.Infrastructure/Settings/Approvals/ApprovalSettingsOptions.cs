using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Approvals;

/// <summary>The "Approvals" settings: how long changes may wait for a second person.</summary>
/// <remarks>Missing values use the defaults (7 days, 24 hours); values outside the ranges stop the API at start-up.</remarks>
public sealed class ApprovalSettingsOptions
{
    /// <summary>The section name.</summary>
    public const string SectionName = AppSettings.Approvals.Section;

    /// <summary>Days a proposal (e.g. new Terms) waits before it expires.</summary>
    [Range(1, 60)]
    public int ProposalDays { get; set; } = 7;

    /// <summary>Hours a shop block waits for confirmation before the shop is unblocked automatically.</summary>
    [Range(1, 168)]
    public int BlockConfirmHours { get; set; } = 24;
}
