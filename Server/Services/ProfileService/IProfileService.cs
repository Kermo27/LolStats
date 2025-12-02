using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.ProfileService;

public interface IProfileService
{
    Task<List<UserProfile>> GetAllAsync();
    Task<UserProfile?> GetByIdAsync(Guid id);
    Task<UserProfile> CreateAsync(UserProfile profile);
    Task<UserProfile> UpdateAsync(UserProfile profile);
    Task<bool> DeleteAsync(Guid id);
}