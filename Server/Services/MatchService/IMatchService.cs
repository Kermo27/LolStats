using LolStatsTracker.API.Models;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.MatchService;

public interface IMatchService
{
    Task<List<MatchEntry>> GetAllAsync(Guid profileId);
    Task<MatchEntry?> GetAsync(Guid id, Guid profileId);
    Task<MatchEntry> AddAsync(MatchEntry match);
    Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match);
    Task<bool> DeleteAsync(Guid id);
    Task ClearAsync(Guid profileId);
}