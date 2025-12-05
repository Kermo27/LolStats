using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.StatsService;

public interface IStatsService
{
    Task<StatsSummaryDto?> GetSummaryAsync(int months = 6, int? seasonId = null);
    Task<List<ChampionStatsDto>> GetChampionsAsync(int? seasonId = null);
    Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role, int? seasonId = null);
    Task<List<DuoSummary>> GetBestDuosAsync(int? seasonId = null);
    Task<List<DuoSummary>> GetWorstEnemyDuosAsync(int? seasonId = null);
}