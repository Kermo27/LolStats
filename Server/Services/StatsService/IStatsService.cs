using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.API.Services.StatsService;

public interface IStatsService
{
    Task<OverviewDto> GetOverviewAsync();
    Task<List<ChampionStatsDto>> GetChampionStatsAsync();
    Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role);
    Task<List<ActivityDayDto>> GetActivityAsync(int months);
    Task<EnchanterUsageSummary> GetEnchanterUsageAsync();
    Task<List<DuoSummary>> GetBestDuosAsync();
    Task<List<DuoSummary>> GetWorstEnemyDuosAsync();
    Task<StatsSummaryDto> GetStatsSummaryAsync(int activityMonths);
}