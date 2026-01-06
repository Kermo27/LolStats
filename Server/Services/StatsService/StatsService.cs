using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.CacheService;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.StatsService;

public class StatsService : IStatsService
{
    private readonly MatchDbContext _db;
    private readonly ICacheService _cache;
    private readonly ILogger<StatsService> _logger;

    public StatsService(MatchDbContext db, ICacheService cache, ILogger<StatsService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    private IQueryable<MatchEntry> GetFilteredQuery(Guid profileId, DateTime? startDate, DateTime? endDate, string? gameMode = null)
    {
        var query = _db.Matches.Where(m => m.ProfileId == profileId);

        if (startDate.HasValue)
            query = query.Where(m => m.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.Date <= endDate.Value);

        if (!string.IsNullOrEmpty(gameMode) && gameMode != "All")
            query = query.Where(m => m.GameMode == gameMode);

        return query.AsNoTracking();
    }

    private async Task<List<MatchEntry>> GetFilteredMatchesAsync(Guid profileId, DateTime? startDate, DateTime? endDate, string? gameMode = null)
    {
        return await GetFilteredQuery(profileId, startDate, endDate, gameMode).ToListAsync();
    }

    public async Task<OverviewDto> GetOverviewAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeOverview(matches);
    }

    public async Task<List<ChampionStatsDto>> GetChampionStatsAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeChampionStats(matches);
    }

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(Guid profileId, string role, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeEnemyStats(matches, role);
    }

    public async Task<List<EnemyStatsDto>> GetHardestEnemiesForRoleAsync(Guid profileId, string playerRole, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeHardestEnemies(matches, playerRole);
    }

    public async Task<List<ActivityDayDto>> GetActivityAsync(Guid profileId, int months, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var query = _db.Matches.Where(m => m.ProfileId == profileId).AsNoTracking();
        var defaultStart = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months));

        if (startDate.HasValue)
            query = query.Where(m => m.Date >= startDate.Value);
        else
            query = query.Where(m => DateOnly.FromDateTime(m.Date) >= defaultStart);

        if (endDate.HasValue)
            query = query.Where(m => m.Date <= endDate.Value);

        if (!string.IsNullOrEmpty(gameMode) && gameMode != "All")
            query = query.Where(m => m.GameMode == gameMode);

        var matches = await query.ToListAsync();

        return matches
            .GroupBy(m => DateOnly.FromDateTime(m.Date))
            .Select(g => new ActivityDayDto(Date: g.Key, GamesPlayed: g.Count()))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<EnchanterUsageSummary> GetEnchanterUsageAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeEnchanterUsage(matches);
    }

    public async Task<List<DuoSummary>> GetBestDuosAsync(Guid profileId, string? playerRole = null, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeBestDuos(matches, playerRole);
    }

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeWorstEnemyDuos(matches);
    }

    public async Task<StreakDto> GetStreakAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeStreak(matches);
    }

    public async Task<TimeAnalysisDto> GetTimeAnalysisAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        return StatsCalculator.ComputeTimeAnalysis(matches);
    }

    public async Task<TiltStatusDto> GetTiltStatusAsync(Guid profileId, string? gameMode = null)
    {
        // TiltStatus needs recent matches regardless of date filters
        var query = _db.Matches.Where(m => m.ProfileId == profileId).AsNoTracking();

        if (!string.IsNullOrEmpty(gameMode) && gameMode != "All")
            query = query.Where(m => m.GameMode == gameMode);

        var matches = await query
            .OrderByDescending(m => m.Date)
            .Take(10)
            .ToListAsync();

        return StatsCalculator.ComputeTiltStatus(matches);
    }
    
    public async Task<StatsSummaryDto> GetStatsSummaryAsync(Guid profileId, int activityMonths = 6, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var cacheKey = CacheKeys.StatsSummary(profileId, null, gameMode);
        
        var cached = await _cache.GetAsync<StatsSummaryDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for stats summary: {ProfileId}", profileId);
            return cached;
        }

        _logger.LogDebug("Cache miss for stats summary: {ProfileId}, computing...", profileId);
        
        var matches = await GetFilteredMatchesAsync(profileId, startDate, endDate, gameMode);
        
        var overview = StatsCalculator.ComputeOverview(matches);
        var championStats = StatsCalculator.ComputeChampionStats(matches);
        var enemyBotStats = StatsCalculator.ComputeEnemyStats(matches, "bot");
        var enemySupportStats = StatsCalculator.ComputeEnemyStats(matches, "support");
        var enchanterUsage = StatsCalculator.ComputeEnchanterUsage(matches);
        var bestDuos = StatsCalculator.ComputeBestDuos(matches, null);
        var worstEnemyDuos = StatsCalculator.ComputeWorstEnemyDuos(matches);
        var streak = StatsCalculator.ComputeStreak(matches);
        var timeAnalysis = StatsCalculator.ComputeTimeAnalysis(matches);
        
        var activity = StatsCalculator.ComputeActivity(matches, activityMonths);

        var tiltStatus = await GetTiltStatusAsync(profileId, gameMode);

        var summary = new StatsSummaryDto(
            Overview: overview,
            ChampionStats: championStats,
            EnemyBotStats: enemyBotStats,
            EnemySupportStats: enemySupportStats,
            Activity: activity,
            EnchanterUsage: enchanterUsage,
            BestDuos: bestDuos,
            WorstEnemyDuos: worstEnemyDuos,
            Streak: streak,
            TimeAnalysis: timeAnalysis,
            TiltStatus: tiltStatus
        );
        
        await _cache.SetAsync(cacheKey, summary, CacheTTL.Short);
        
        return summary;
    }
    
    public async Task InvalidateCacheAsync(Guid profileId)
    {
        _logger.LogDebug("Invalidating stats cache for profile: {ProfileId}", profileId);
        
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, null));
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, "Ranked Solo"));
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, "Ranked Flex"));
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, "All"));
    }
}