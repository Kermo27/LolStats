using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.StatsService;

public interface IStatsService
{
    Task<StatsSummaryDto?> GetSummaryAsync(int months = 6);
    Task<List<ChampionStatsDto>> GetChampionsAsync();
    Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role);
    Task<List<DuoSummary>> GetBestDuosAsync();
    Task<List<DuoSummary>> GetWorstEnemyDuosAsync();
}