using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.ProfileService;

public interface IProfileService
{
    Task<List<UserProfile>> GetAllAsync(Guid userId);
    Task<UserProfile?> GetByIdAsync(Guid id, Guid userId);
    Task<UserProfile> CreateAsync(UserProfile profile, Guid userId);
    Task<UserProfile> UpdateAsync(UserProfile profile, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}