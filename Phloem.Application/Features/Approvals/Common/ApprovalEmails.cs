using System.Net;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Approvals.Common;

/// <summary>
/// Emails about two-person approval: "a change is waiting for you" to everyone who may decide it, and "your change
/// was approved / rejected / expired" to the person who proposed it. Never fails the request.
/// </summary>
public sealed class ApprovalEmails(
    IUserRoleService roles,
    IStaffCountryRepository staffCountries,
    IAccountService accounts,
    IUserProfileRepository profiles,
    IEmailSender email,
    ILogger<ApprovalEmails> logger)
{
    /// <summary>Tells everyone who may decide it that a change is waiting.</summary>
    /// <param name="change">The new proposal (saved).</param>
    /// <param name="ct">Stops the sending.</param>
    public async Task WaitingAsync(PendingChange change, CancellationToken ct)
    {
        try
        {
            var names = await profiles.GetFullNamesAsync([change.ProposedById], ct);
            var who = names.GetValueOrDefault(change.ProposedById, "A staff member");
            var deadline = change.Kind == PendingChangeKind.ConfirmShopBlock
                ? $"If nobody confirms it by {change.ExpiresAtUtc:d MMM yyyy, HH:mm} UTC, the shop is unblocked automatically."
                : $"It expires if nobody decides by {change.ExpiresAtUtc:d MMM yyyy, HH:mm} UTC.";

            foreach (var id in await DecidersAsync(change, ct))
            {
                var contact = await accounts.GetContactAsync(id, ct);
                if (contact.IsFailure) continue;

                await email.SendAsync(contact.Value.Email, $"Waiting for your approval: {change.Summary}",
                    $"<p><b>{WebUtility.HtmlEncode(who)}</b> asked for a second person to approve:</p>" +
                    $"<blockquote>{WebUtility.HtmlEncode(change.Summary)}</blockquote>" +
                    $"<p>{WebUtility.HtmlEncode(deadline)}</p>" +
                    "<p>Open <b>Staff area → Approvals</b> on the Phloem website to approve or reject it.</p>",
                    EmailReplyTo.Support, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email the approvers of change {ChangeId}.", change.Id);
        }
    }

    /// <summary>Tells the proposer what happened to their change.</summary>
    /// <param name="change">The decided (or expired) change.</param>
    /// <param name="ct">Stops the sending.</param>
    public async Task DecidedAsync(PendingChange change, CancellationToken ct)
    {
        try
        {
            var contact = await accounts.GetContactAsync(change.ProposedById, ct);
            if (contact.IsFailure) return;

            var by = change.DecidedById is { } d
                ? (await profiles.GetFullNamesAsync([d], ct)).GetValueOrDefault(d, "another staff member")
                : null;
            var what = change.Status switch
            {
                PendingChangeStatus.Approved => $"was approved by {by}",
                PendingChangeStatus.Rejected => $"was rejected by {by}",
                PendingChangeStatus.Expired => "expired: nobody decided in time",
                PendingChangeStatus.Failed => $"was approved by {by}, but couldn't be applied",
                _ => null
            };
            if (what is null) return;

            await email.SendAsync(contact.Value.Email, $"{change.Summary}: {change.Status.ToString().ToLowerInvariant()}",
                $"<p>Your change <b>{WebUtility.HtmlEncode(change.Summary)}</b> {WebUtility.HtmlEncode(what)}.</p>" +
                (change.DecisionNote is { } note ? $"<p>Reason: {WebUtility.HtmlEncode(note)}</p>" : string.Empty) +
                (change.Outcome is { } outcome ? $"<p>{WebUtility.HtmlEncode(outcome)}</p>" : string.Empty),
                EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email the proposer of change {ChangeId}.", change.Id);
        }
    }

    /// <summary>Owners, and (for shop blocks) Country managers covering the shop's country; never the proposer.</summary>
    private async Task<IReadOnlyCollection<Guid>> DecidersAsync(PendingChange change, CancellationToken ct)
    {
        var ids = (await roles.GetUserIdsInRoleAsync(Roles.Admin, ct)).ToHashSet();

        if (change.Kind == PendingChangeKind.ConfirmShopBlock && change.Country is { } country)
        {
            var managers = await roles.GetUserIdsInRoleAsync(Roles.CountryManager, ct);
            var rows = await staffCountries.GetForUsersAsync(managers, ct);
            foreach (var (id, list) in rows)
                if (list.Any(r => r.IsAllCountries || r.Country == country)) ids.Add(id);
        }

        ids.Remove(change.ProposedById);
        return ids;
    }
}
