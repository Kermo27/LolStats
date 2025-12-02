using LolStatsTracker.API.Data;
using LolStatsTracker.API.Models;
using LolStatsTracker.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.StatsService;

public class StatsService : IStatsService
{
    private readonly MatchDbContext _db;
    
    public StatsService(MatchDbContext db)
    {
        _db = db;
    }
    
    public async Task<OverviewDto> GetOverviewAsync(Guid profileId)
    {
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .ToListAsync();
        if (!matches.Any()) return new OverviewDto(0, "", 0, "", 0);
        
        var winrate = matches.Count(m => m.Win) / (double)matches.Count;
        
        var mostPlayed = matches
            .GroupBy(m => m.Champion)
            .OrderByDescending(g => g.Count())
            .First();

        var favSupport = matches
            .Where(m => !string.IsNullOrEmpty(m.Support))
            .GroupBy(m => m.Support)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return new OverviewDto(
            Winrate: winrate,
            MostPlayedChampion: mostPlayed.Key,
            MostPlayedChampionGames: mostPlayed.Count(),
            FavoriteSupport: favSupport?.Key ?? "",
            FavoriteSupportGames: favSupport?.Count() ?? 0
        );
    }

    public async Task<List<ChampionStatsDto>> GetChampionStatsAsync(Guid profileId)
    {
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .ToListAsync();

        return matches
            .GroupBy(m => m.Champion)
            .Select(g => {
                var games = g.Count();
                var wins = g.Count(m => m.Win);
                return new ChampionStatsDto(
                    ChampionName: g.Key,
                    Games: games,
                    Wins: wins,
                    Losses: games - wins,
                    Winrate: (double)wins / games
                );
            })
            .OrderByDescending(x => x.Games)
            .ToList();
    }

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(Guid profileId, string role)
    {
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .Where(m => m.Role == "ADC")
            .ToListAsync();

        return matches
            .Where(m => role.ToLower() == "bot" ? !string.IsNullOrEmpty(m.EnemyBot) : !string.IsNullOrEmpty(m.EnemySupport))
            .GroupBy(m => role.ToLower() == "bot" ? m.EnemyBot : m.EnemySupport)
            .Select(g => new EnemyStatsDto(
                ChampionName: g.Key,
                Games: g.Count(),
                WinrateAgainst: g.Count(m => !m.Win) / (double)g.Count()
            ))
            .OrderByDescending(x => x.WinrateAgainst)
            .ToList();
    }

    public async Task<List<ActivityDayDto>> GetActivityAsync(Guid profileId, int months)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months));
        var matches = await _db.Matches
            .Where(m => DateOnly.FromDateTime(m.Date) >= startDate)
            .ToListAsync();

        return matches
            .GroupBy(m => DateOnly.FromDateTime(m.Date))
            .Select(g => new ActivityDayDto(
                Date: g.Key,
                GamesPlayed: g.Count()
            ))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<EnchanterUsageSummary> GetEnchanterUsageAsync(Guid profileId)
    {
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .ToListAsync();
        var enchanters = ChampionList.Enchanters;

        var mySupportGames = matches.Count(m => !string.IsNullOrWhiteSpace(m.Support));
        var enemySupportGames = matches.Count(m => !string.IsNullOrWhiteSpace(m.EnemySupport));

        var myEnchanterGames = matches.Count(m => enchanters.Contains(m.Support));
        var enemyEnchanterGames = matches.Count(m => enchanters.Contains(m.EnemySupport));

        return new EnchanterUsageSummary(
            MyEnchanterGames: myEnchanterGames,
            MyPercentage: mySupportGames == 0 ? 0 : (double)myEnchanterGames / mySupportGames * 100,
            EnemyEnchanterGames: enemyEnchanterGames,
            EnemyPercentage: enemySupportGames == 0 ? 0 : (double)enemyEnchanterGames / enemySupportGames * 100
        );
    }

    public async Task<List<DuoSummary>> GetBestDuosAsync(Guid profileId)
    {
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .Where(m => m.Role == "ADC")
            .ToListAsync();

        return matches
            .GroupBy(m => (m.Champion, m.Support))
            .Select(g => new DuoSummary
            {
                Champion = g.Key.Item1,
                Support = g.Key.Item2,
                Count = g.Count(),
                WinRate = g.Average(m => m.Win ? 1 : 0) * 100,
                AvgKda = g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths))
            })
            .Where(d => d.Count >= 2)
            .OrderByDescending(d => d.WinRate)
            .Take(3)
            .ToList();
    }

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync(Guid profileId)
    {
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .Where(m => m.Role == "ADC")
            .ToListAsync();

        return matches
            .GroupBy(m => (m.EnemyBot, m.EnemySupport))
            .Select(g => new DuoSummary
            {
                Champion = g.Key.Item1,
                Support = g.Key.Item2,
                Count = g.Count(),
                WinRate = g.Average(m => m.Win ? 1 : 0) * 100,
                AvgKda = g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths))
            })
            .Where(d => d.Count >= 2)
            .OrderBy(d => d.WinRate)
            .Take(3)
            .ToList();
    }

    public async Task<StatsSummaryDto> GetStatsSummaryAsync(Guid profileId, int activityMonths = 6)
    {
        var overviewTask = GetOverviewAsync(profileId);
        var championStatsTask = GetChampionStatsAsync(profileId);
        var enemyBotTask = GetEnemyStatsAsync(profileId, "bot");
        var enemySupportTask = GetEnemyStatsAsync(profileId, "support");
        var activityTask = GetActivityAsync(profileId, activityMonths);
        var enchanterTask = GetEnchanterUsageAsync(profileId);
        var bestDuosTask = GetBestDuosAsync(profileId);
        var worstEnemyDuosTask = GetWorstEnemyDuosAsync(profileId);

        await Task.WhenAll(overviewTask, championStatsTask, enemyBotTask, enemySupportTask,
            activityTask, enchanterTask, bestDuosTask, worstEnemyDuosTask);

        return new StatsSummaryDto(
            Overview: await overviewTask,
            ChampionStats: await championStatsTask,
            EnemyBotStats: await enemyBotTask,
            EnemySupportStats: await enemySupportTask,
            Activity: await activityTask,
            EnchanterUsage: await enchanterTask,
            BestDuos: await bestDuosTask,
            WorstEnemyDuos: await worstEnemyDuosTask
        );
    }

}