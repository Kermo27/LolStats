using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.StatsService;

public interface IStatsService
{
    Task<StatsSummaryDto?> GetSummaryAsync(int months = 6, int? seasonId = null, string? gameMode = null);
    Task<List<ChampionStatsDto>> GetChampionsAsync(int? seasonId = null, string? gameMode = null);
    Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role, int? seasonId = null, string? gameMode = null);
    Task<List<EnemyStatsDto>> GetHardestEnemiesAsync(string? playerRole = null, int? seasonId = null, string? gameMode = null);
    Task<List<DuoSummary>> GetBestDuosAsync(string? playerRole = null, int? seasonId = null, string? gameMode = null);
    Task<List<DuoSummary>> GetWorstEnemyDuosAsync(int? seasonId = null, string? gameMode = null);
}