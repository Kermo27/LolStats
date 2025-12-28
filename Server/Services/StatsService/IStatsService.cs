using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.API.Services.StatsService;

public interface IStatsService
{
    Task<OverviewDto> GetOverviewAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<List<ChampionStatsDto>> GetChampionStatsAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<List<EnemyStatsDto>> GetEnemyStatsAsync(Guid profileId, string role, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<List<ActivityDayDto>> GetActivityAsync(Guid profileId, int months, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<EnchanterUsageSummary> GetEnchanterUsageAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<List<DuoSummary>> GetBestDuosAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<List<DuoSummary>> GetWorstEnemyDuosAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<StreakDto> GetStreakAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<TimeAnalysisDto> GetTimeAnalysisAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<TiltStatusDto> GetTiltStatusAsync(Guid profileId, string? gameMode = null);
    Task<StatsSummaryDto> GetStatsSummaryAsync(Guid profileId, int activityMonths, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
}