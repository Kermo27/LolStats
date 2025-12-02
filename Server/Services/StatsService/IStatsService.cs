using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.API.Services.StatsService;

public interface IStatsService
{
    Task<OverviewDto> GetOverviewAsync(Guid profileId);
    Task<List<ChampionStatsDto>> GetChampionStatsAsync(Guid profileId);
    Task<List<EnemyStatsDto>> GetEnemyStatsAsync(Guid profileId, string role);
    Task<List<ActivityDayDto>> GetActivityAsync(Guid profileId, int months);
    Task<EnchanterUsageSummary> GetEnchanterUsageAsync(Guid profileId);
    Task<List<DuoSummary>> GetBestDuosAsync(Guid profileId);
    Task<List<DuoSummary>> GetWorstEnemyDuosAsync(Guid profileId);
    Task<StatsSummaryDto> GetStatsSummaryAsync(Guid profileId, int activityMonths);
}