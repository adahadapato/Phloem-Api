using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>Moderation checks: the background checker's work, and the queue on Admin → Moderation.</summary>
public interface IModerationCheckRepository : IRepository<ModerationCheck>
{
    /// <summary>Checks waiting for the AI, oldest first (tracked, to change them).</summary>
    /// <param name="take">Most to return.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<ModerationCheck>> GetWaitingForAiAsync(int take, CancellationToken cancellationToken = default);

    /// <summary>The queue: checks waiting for a person, oldest first (read-only).</summary>
    /// <param name="kind">Only this kind; null = all.</param>
    /// <param name="countries">Only these countries (and checks without one); null = all.</param>
    /// <param name="page">Which page.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<PagedResult<ModerationCheck>> GetWaitingForPersonAsync(ModerationKind? kind,
                                                                IReadOnlyCollection<Country>? countries,
                                                                PageRequest page,
                                                                CancellationToken cancellationToken = default);

    /// <summary>How many wait for a person, by kind (for the menu number).</summary>
    /// <param name="countries">Only these countries (and checks without one); null = all.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyDictionary<ModerationKind, int>> CountWaitingForPersonAsync(IReadOnlyCollection<Country>? countries,
                                                                              CancellationToken cancellationToken = default);

    /// <summary>The newest check of each target (read-only), e.g. the AI's advice for a page of dish names.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="targetIds">The targets.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyDictionary<Guid, ModerationCheck>> GetLatestAsync(ModerationKind kind,
                                                                    IReadOnlyCollection<Guid> targetIds,
                                                                    CancellationToken cancellationToken = default);

    /// <summary>Unfinished checks of one target (tracked), to replace them when the text changes again.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="targetId">The target.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<ModerationCheck>> GetOpenForTargetAsync(ModerationKind kind, Guid targetId,
                                                               CancellationToken cancellationToken = default);

    /// <summary>Dishes (not deleted) that have never been checked: the ones that existed before checks began.</summary>
    /// <param name="take">Most to return.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    /// <returns>Id, text, country.</returns>
    Task<IReadOnlyList<(Guid Id, string Text, Country Country)>> GetUncheckedDishesAsync(
        int take, CancellationToken cancellationToken = default);

    /// <summary>Shops whose name has never been checked.</summary>
    /// <param name="take">Most to return.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    /// <returns>Id, name, country.</returns>
    Task<IReadOnlyList<(Guid Id, string Name, Country Country)>> GetUncheckedShopsAsync(
        int take, CancellationToken cancellationToken = default);
}
