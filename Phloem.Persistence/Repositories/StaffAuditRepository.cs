using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IStaffAuditRepository"/> with EF Core (add only, for now).</summary>
internal sealed class StaffAuditRepository(PhloemDbContext context)
    : Repository<StaffAuditEntry>(context), IStaffAuditRepository;
