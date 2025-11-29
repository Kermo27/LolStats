using LolStatsTracker.API.Models;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.MatchService;

public interface IMatchService
{
    Task<List<MatchEntry>> GetAllAsync();
    Task<MatchEntry?> GetAsync(Guid id);
    Task<MatchEntry> AddAsync(MatchEntry match);
    Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match);
    Task DeleteAsync(Guid id);
    Task ClearAsync();
}