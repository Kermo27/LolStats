using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.API.Services.AdminService;

public interface IAdminService
{
    Task<List<UserListDto>> GetAllUsersAsync();
    Task<bool> UpdateUserRoleAsync(Guid userId, string role);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<SystemStatsDto> GetSystemStatsAsync();
    Task<PaginatedResponse<AdminMatchDto>> GetAllMatchesAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<bool> DeleteMatchAsync(Guid matchId);
    Task<List<ProfileListDto>> GetAllProfilesAsync();
    Task<List<ProfileListDto>> GetProfilesByUserIdAsync(Guid userId);
}
