using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Base for every Domain entity configuration. Maps what <see cref="Entity{TId}"/> gives every table
/// (Id, audit columns, soft delete), then calls <see cref="ConfigureEntity"/> for the entity's own columns.
/// </summary>
/// <typeparam name="TEntity">The Domain entity.</typeparam>
/// <remarks>
/// <para><b>Id:</b> <c>ValueGeneratedNever</c> on EVERY table. The Domain creates its own Ids
/// (<c>Guid.CreateVersion7()</c>, or the Identity user's Id for UserProfile). Without this, EF Core assumes a child
/// added to a loaded aggregate (a new address, order line, rate…) already exists and sends an UPDATE instead of an
/// INSERT.</para>
/// <para><b>Audit:</b> <c>CreatedAt</c> / <c>UpdatedAt</c> are filled by the audit interceptor when saving, never by
/// Domain code.</para>
/// <para><b>Soft delete:</b> a global filter hides rows with <c>IsDeleted = true</c> from every query. Use
/// <c>IgnoreQueryFilters()</c> when deleted rows are really needed (e.g. admin reports). The filter also applies
/// through navigations, so repositories must NOT <c>Include</c> a required parent that can be soft-deleted (e.g. an
/// order's customer): that would hide the child too. Read such names with the batch lookups instead
/// (e.g. <c>GetFullNamesAsync</c>).</para>
/// </remarks>
public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<Guid>
{
    /// <summary>Maps the shared columns, then the entity's own.</summary>
    /// <param name="builder">EF Core's builder for this entity.</param>
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.DeletedAt);
        builder.HasQueryFilter(e => !e.IsDeleted);

        ConfigureEntity(builder);
    }

    /// <summary>The entity's own table, columns, relationships and indexes.</summary>
    /// <param name="builder">EF Core's builder for this entity.</param>
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}