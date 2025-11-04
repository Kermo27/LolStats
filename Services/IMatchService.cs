using LolStatsTracker.Models;

namespace LolStatsTracker.Services;

public interface IMatchService
{
    Task<List<MatchEntry>> GetAllAsync();
    Task<MatchEntry?> GetAsync(Guid id);
    Task<MatchEntry> AddAsync(MatchEntry match);
    Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match);
    Task DeleteAsync(Guid id);
    Task ClearAsync();
    Task<IEnumerable<MatchStats>> GetChampionStatsAsync();
    Task<IEnumerable<SupportStats>> GetSupportStatsAsync();
    Task<IEnumerable<EnemyStats>> GetEnemyBotStatsAsync();
    Task<IEnumerable<EnemySupportStats>> GetEnemySupportStatsAsync();
}