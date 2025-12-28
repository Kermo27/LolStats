using LolStatsTracker.API.Data;
using LolStatsTracker.API.Models;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.StatsService;

public class StatsService : IStatsService
{
    private readonly MatchDbContext _db;
    
    public StatsService(MatchDbContext db)
    {
        _db = db;
    }

    private IQueryable<MatchEntry> GetFilteredMatches(Guid profileId, DateTime? startDate, DateTime? endDate)
    {
        var query = _db.Matches.Where(m => m.ProfileId == profileId);
        
        if (startDate.HasValue)
            query = query.Where(m => m.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(m => m.Date <= endDate.Value);
        
        return query;
    }
    
    public async Task<OverviewDto> GetOverviewAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate).ToListAsync();
        if (!matches.Any()) return new OverviewDto(0, "", 0, "", 0);
        
        var winrate = matches.Count(m => m.Win) / (double)matches.Count;
        
        var mostPlayed = matches
            .GroupBy(m => m.Champion)
            .OrderByDescending(g => g.Count())
            .First();

        var favSupport = matches
            .Where(m => m.Role == "ADC" && !string.IsNullOrEmpty(m.LaneAlly))
            .GroupBy(m => m.LaneAlly)
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

    public async Task<List<ChampionStatsDto>> GetChampionStatsAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate).ToListAsync();

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

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(Guid profileId, string role, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate)
            .Where(m => m.Role == "ADC")
            .ToListAsync();

        return matches
            .Where(m => role.ToLower() == "bot" ? !string.IsNullOrEmpty(m.LaneEnemy) : !string.IsNullOrEmpty(m.LaneEnemyAlly))
            .GroupBy(m => role.ToLower() == "bot" ? m.LaneEnemy : m.LaneEnemyAlly)
            .Select(g => new EnemyStatsDto(
                ChampionName: g.Key,
                Games: g.Count(),
                WinrateAgainst: g.Count(m => !m.Win) / (double)g.Count()
            ))
            .OrderByDescending(x => x.WinrateAgainst)
            .ToList();
    }

    public async Task<List<ActivityDayDto>> GetActivityAsync(Guid profileId, int months, DateTime? startDate = null, DateTime? endDate = null)
    {
        var defaultStart = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months));
        
        var query = _db.Matches.Where(m => m.ProfileId == profileId);
        
        if (startDate.HasValue)
            query = query.Where(m => m.Date >= startDate.Value);
        else
            query = query.Where(m => DateOnly.FromDateTime(m.Date) >= defaultStart);
        
        if (endDate.HasValue)
            query = query.Where(m => m.Date <= endDate.Value);
        
        var matches = await query.ToListAsync();

        return matches
            .GroupBy(m => DateOnly.FromDateTime(m.Date))
            .Select(g => new ActivityDayDto(
                Date: g.Key,
                GamesPlayed: g.Count()
            ))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<EnchanterUsageSummary> GetEnchanterUsageAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate)
            .Where(m => m.Role == "ADC")
            .ToListAsync();
        var enchanters = ChampionList.Enchanters;

        var mySupportGames = matches.Count(m => !string.IsNullOrWhiteSpace(m.LaneAlly));
        var enemySupportGames = matches.Count(m => !string.IsNullOrWhiteSpace(m.LaneEnemyAlly));

        var myEnchanterGames = matches.Count(m => enchanters.Contains(m.LaneAlly));
        var enemyEnchanterGames = matches.Count(m => enchanters.Contains(m.LaneEnemyAlly));

        return new EnchanterUsageSummary(
            MyEnchanterGames: myEnchanterGames,
            MyPercentage: mySupportGames == 0 ? 0 : (double)myEnchanterGames / mySupportGames * 100,
            EnemyEnchanterGames: enemyEnchanterGames,
            EnemyPercentage: enemySupportGames == 0 ? 0 : (double)enemyEnchanterGames / enemySupportGames * 100
        );
    }

    public async Task<List<DuoSummary>> GetBestDuosAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate)
            .Where(m => m.Role == "ADC")
            .ToListAsync();

        return matches
            .GroupBy(m => (m.Champion, m.LaneAlly))
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

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate)
            .Where(m => m.Role == "ADC")
            .ToListAsync();

        return matches
            .GroupBy(m => (m.LaneEnemy, m.LaneEnemyAlly))
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

    public async Task<StreakDto> GetStreakAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate)
            .OrderByDescending(m => m.Date)
            .ToListAsync();

        if (!matches.Any())
        {
            return new StreakDto
            {
                IsWinStreak = true,
                Count = 0,
                BestWinStreak = 0,
                WorstLossStreak = 0,
                TotalGames = 0
            };
        }

        // Calculate current streak
        var currentStreak = 0;
        var isWinStreak = matches.First().Win;
        
        foreach (var match in matches)
        {
            if (match.Win == isWinStreak)
                currentStreak++;
            else
                break;
        }
        
        var bestWinStreak = 0;
        var worstLossStreak = 0;
        var tempStreak = 0;
        bool? lastResult = null;

        foreach (var match in matches.OrderBy(m => m.Date))
        {
            if (lastResult == null || match.Win == lastResult)
            {
                tempStreak++;
            }
            else
            {
                if (lastResult == true && tempStreak > bestWinStreak)
                    bestWinStreak = tempStreak;
                if (lastResult == false && tempStreak > worstLossStreak)
                    worstLossStreak = tempStreak;
                
                tempStreak = 1;
            }
            lastResult = match.Win;
        }
        
        if (lastResult == true && tempStreak > bestWinStreak)
            bestWinStreak = tempStreak;
        if (lastResult == false && tempStreak > worstLossStreak)
            worstLossStreak = tempStreak;

        return new StreakDto
        {
            IsWinStreak = isWinStreak,
            Count = currentStreak,
            BestWinStreak = bestWinStreak,
            WorstLossStreak = worstLossStreak,
            TotalGames = matches.Count
        };
    }

    public async Task<TimeAnalysisDto> GetTimeAnalysisAsync(Guid profileId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var matches = await GetFilteredMatches(profileId, startDate, endDate).ToListAsync();
        
        if (!matches.Any())
        {
            return new TimeAnalysisDto();
        }

        var dayNames = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        
        var byHour = Enumerable.Range(0, 24)
            .Select(hour =>
            {
                var hourMatches = matches.Where(m => m.Date.Hour == hour).ToList();
                var games = hourMatches.Count;
                var wins = hourMatches.Count(m => m.Win);
                return new HourWinrateDto(hour, games, wins, games > 0 ? (double)wins / games : 0);
            })
            .ToList();

        var byDay = Enumerable.Range(0, 7)
            .Select(dayIndex =>
            {
                var dayMatches = matches.Where(m => (int)m.Date.DayOfWeek == dayIndex).ToList();
                var games = dayMatches.Count;
                var wins = dayMatches.Count(m => m.Win);
                return new DayWinrateDto(dayNames[dayIndex], dayIndex, games, wins, games > 0 ? (double)wins / games : 0);
            })
            .ToList();

        var bestHour = byHour.Where(h => h.Games >= 3).OrderByDescending(h => h.Winrate).FirstOrDefault();
        var bestDay = byDay.Where(d => d.Games >= 3).OrderByDescending(d => d.Winrate).FirstOrDefault();

        return new TimeAnalysisDto
        {
            ByHour = byHour,
            ByDayOfWeek = byDay,
            BestHourRange = bestHour != null ? $"{bestHour.Hour}:00 - {bestHour.Hour + 1}:00" : "N/A",
            BestDay = bestDay?.Day ?? "N/A",
            BestHourWinrate = bestHour?.Winrate ?? 0,
            BestDayWinrate = bestDay?.Winrate ?? 0
        };
    }

    public async Task<TiltStatusDto> GetTiltStatusAsync(Guid profileId)
    {
        var today = DateTime.Today;
        var recentMatches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .OrderByDescending(m => m.Date)
            .Take(10)
            .ToListAsync();

        if (!recentMatches.Any())
        {
            return new TiltStatusDto { Message = "Play some games to see your tilt status!" };
        }

        var last5 = recentMatches.Take(5).ToList();
        var todayMatches = recentMatches.Where(m => m.Date.Date == today).ToList();
        
        var recentLosses = 0;
        foreach (var match in recentMatches)
        {
            if (!match.Win) recentLosses++;
            else break;
        }

        var last5Winrate = last5.Count > 0 ? last5.Count(m => m.Win) / (double)last5.Count : 0;
        var todayLosses = todayMatches.Count(m => !m.Win);

        TiltLevel level;
        string message;

        if (recentLosses >= 4 || (todayMatches.Count >= 4 && todayLosses >= 3))
        {
            level = TiltLevel.Critical;
            message = "🚨 STOP! Take a break. You're on a major losing streak.";
        }
        else if (recentLosses >= 3 || (last5.Count >= 5 && last5Winrate < 0.3))
        {
            level = TiltLevel.Danger;
            message = "⚠️ Consider taking a break. Recent performance is rough.";
        }
        else if (recentLosses >= 2)
        {
            level = TiltLevel.Warning;
            message = "😤 2 losses in a row. Stay focused or take a short break.";
        }
        else
        {
            level = TiltLevel.None;
            message = "✅ You're playing well! Keep it up.";
        }

        return new TiltStatusDto
        {
            IsTilted = level >= TiltLevel.Warning,
            RecentLosses = recentLosses,
            RecentGames = last5.Count,
            RecentWinrate = last5Winrate,
            Message = message,
            Level = level
        };
    }

    public async Task<StatsSummaryDto> GetStatsSummaryAsync(Guid profileId, int activityMonths = 6, DateTime? startDate = null, DateTime? endDate = null)
    {
        var overviewTask = GetOverviewAsync(profileId, startDate, endDate);
        var championStatsTask = GetChampionStatsAsync(profileId, startDate, endDate);
        var enemyBotTask = GetEnemyStatsAsync(profileId, "bot", startDate, endDate);
        var enemySupportTask = GetEnemyStatsAsync(profileId, "support", startDate, endDate);
        var activityTask = GetActivityAsync(profileId, activityMonths, startDate, endDate);
        var enchanterTask = GetEnchanterUsageAsync(profileId, startDate, endDate);
        var bestDuosTask = GetBestDuosAsync(profileId, startDate, endDate);
        var worstEnemyDuosTask = GetWorstEnemyDuosAsync(profileId, startDate, endDate);
        var streakTask = GetStreakAsync(profileId, startDate, endDate);
        var timeAnalysisTask = GetTimeAnalysisAsync(profileId, startDate, endDate);
        var tiltStatusTask = GetTiltStatusAsync(profileId);

        await Task.WhenAll(overviewTask, championStatsTask, enemyBotTask, enemySupportTask,
            activityTask, enchanterTask, bestDuosTask, worstEnemyDuosTask, streakTask, timeAnalysisTask, tiltStatusTask);

        return new StatsSummaryDto(
            Overview: await overviewTask,
            ChampionStats: await championStatsTask,
            EnemyBotStats: await enemyBotTask,
            EnemySupportStats: await enemySupportTask,
            Activity: await activityTask,
            EnchanterUsage: await enchanterTask,
            BestDuos: await bestDuosTask,
            WorstEnemyDuos: await worstEnemyDuosTask,
            Streak: await streakTask,
            TimeAnalysis: await timeAnalysisTask,
            TiltStatus: await tiltStatusTask
        );
    }
}