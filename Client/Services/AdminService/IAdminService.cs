using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.AdminService;

public interface IAdminService
{
    Task<SystemStatsDto?> GetStatsAsync();
    Task<List<UserListDto>> GetUsersAsync();
    Task<bool> UpdateUserRoleAsync(Guid userId, string role);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<PaginatedResponse<AdminMatchDto>?> GetMatchesAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<bool> DeleteMatchAsync(Guid matchId);
    Task<List<ProfileListDto>> GetProfilesAsync();
    Task<List<ProfileListDto>> GetProfilesByUserIdAsync(Guid userId);
}
