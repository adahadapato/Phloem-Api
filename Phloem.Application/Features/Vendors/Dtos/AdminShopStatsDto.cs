namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>Admin dashboard tiles: shops per status (closed shops aren't counted).</summary>
public sealed record AdminShopStatsDto(int PendingApproval, int Approved, int Rejected, int Blocked, int Total);
