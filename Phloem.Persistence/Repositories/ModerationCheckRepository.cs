using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IModerationCheckRepository"/> with EF Core.</summary>
internal sealed class ModerationCheckRepository(PhloemDbContext context)
    : Repository<ModerationCheck>(context), IModerationCheckRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ModerationCheck>> GetWaitingForAiAsync(int take,
                                                                          CancellationToken cancellationToken = default) =>
        await Set.Where(c => c.State == ModerationState.WaitingForAi)
                 .OrderBy(c => c.CreatedAt)
                 .Take(take)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<PagedResult<ModerationCheck>> GetWaitingForPersonAsync(ModerationKind? kind,
                                                                       IReadOnlyCollection<Country>? countries,
                                                                       PageRequest page,
                                                                       CancellationToken cancellationToken = default)
    {
        var query = Waiting(countries);
        if (kind is { } k) query = query.Where(c => c.Kind == k);
        return ToPagedAsync(query.OrderBy(c => c.CreatedAt), page, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<ModerationKind, int>> CountWaitingForPersonAsync(
        IReadOnlyCollection<Country>? countries, CancellationToken cancellationToken = default) =>
        await Waiting(countries).GroupBy(c => c.Kind)
                                .Select(g => new { g.Key, Count = g.Count() })
                                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, ModerationCheck>> GetLatestAsync(ModerationKind kind,
                                                                                 IReadOnlyCollection<Guid> targetIds,
                                                                                 CancellationToken cancellationToken = default)
    {
        if (targetIds.Count == 0) return new Dictionary<Guid, ModerationCheck>();
        var ids = targetIds.ToList();
        var rows = await Set.AsNoTracking()
                            .Where(c => c.Kind == kind && ids.Contains(c.TargetId) && c.State != ModerationState.Replaced)
                            .ToListAsync(cancellationToken);
        return rows.GroupBy(c => c.TargetId).ToDictionary(g => g.Key, g => g.MaxBy(c => c.CreatedAt)!);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModerationCheck>> GetOpenForTargetAsync(ModerationKind kind, Guid targetId,
                                                                            CancellationToken cancellationToken = default) =>
        await Set.Where(c => c.Kind == kind && c.TargetId == targetId
                          && (c.State == ModerationState.WaitingForAi || c.State == ModerationState.WaitingForPerson))
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<(Guid Id, string Text, Country Country)>> GetUncheckedDishesAsync(
        int take, CancellationToken cancellationToken = default)
    {
        var rows = await Context.Set<Dish>().AsNoTracking()
            .Where(d => !Set.Any(c => c.Kind == ModerationKind.Dish && c.TargetId == d.Id))
            .OrderBy(d => d.CreatedAt)
            .Take(take)
            .Select(d => new { d.Id, d.Name, d.Description, d.Country })
            .ToListAsync(cancellationToken);
        return rows.Select(r => (r.Id, string.IsNullOrWhiteSpace(r.Description) ? r.Name : $"{r.Name}\n{r.Description}",
                                 r.Country)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(Guid Id, string Name, Country Country)>> GetUncheckedShopsAsync(
        int take, CancellationToken cancellationToken = default)
    {
        var rows = await Context.Set<Vendor>().AsNoTracking()
            .Where(v => !Set.Any(c => c.Kind == ModerationKind.ShopName && c.TargetId == v.Id))
            .OrderBy(v => v.CreatedAt)
            .Take(take)
            .Select(v => new { v.Id, v.Name, v.Country })
            .ToListAsync(cancellationToken);
        return rows.Select(r => (r.Id, r.Name, r.Country)).ToList();
    }

    private IQueryable<ModerationCheck> Waiting(IReadOnlyCollection<Country>? countries)
    {
        IQueryable<ModerationCheck> query = Set.AsNoTracking().Where(c => c.State == ModerationState.WaitingForPerson);
        if (countries is not null)
        {
            var covered = countries.ToList();
            query = query.Where(c => c.Country == null || covered.Contains(c.Country.Value));
        }
        return query;
    }
}
