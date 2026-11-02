using Phloem.Domain.Entities;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// The staff audit log. Lines are only ever added (<see cref="IRepository{TEntity}.Add"/>); the owners' page for
/// reading them comes with the Staff page.
/// </summary>
public interface IStaffAuditRepository : IRepository<StaffAuditEntry>;
