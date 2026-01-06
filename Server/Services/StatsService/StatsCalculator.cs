using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.StatsService;

public static class StatsCalculator
{
    private static readonly string[] DayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    
    private static readonly HashSet<string> Enchanters = new(StringComparer.OrdinalIgnoreCase)
    {
        "Janna", "Karma", "Lulu", "Milio", "Nami", "Renata Glasc", "Senna", "Seraphine", "Sona", "Soraka", "Taric", "Yuumi"
    };

    public static OverviewDto ComputeOverview(List<MatchEntry> matches)
    {
        if (matches.Count == 0)
            return new OverviewDto(0, "", 0, "", 0);

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

    public static List<ChampionStatsDto> ComputeChampionStats(List<MatchEntry> matches)
    {
        if (matches.Count == 0)
            return new List<ChampionStatsDto>();

        return matches
            .GroupBy(m => m.Champion)
            .Select(g =>
            {
                var games = g.Count();
                var wins = g.Count(m => m.Win);
                var mostPlayedRole = g.GroupBy(m => m.Role)
                    .OrderByDescending(r => r.Count())
                    .First().Key;

                var avgKda = g.Average(m => m.Deaths == 0
                    ? m.Kills + m.Assists
                    : (double)(m.Kills + m.Assists) / m.Deaths);

                var avgCsm = g.Average(m => m.GameLengthMinutes > 0
                    ? (double)m.Cs / m.GameLengthMinutes
                    : 0);

                var avgVisionScore = g.Average(m => m.VisionScore ?? 0);

                return new ChampionStatsDto(
                    ChampionName: g.Key,
                    Role: mostPlayedRole,
                    Games: games,
                    Wins: wins,
                    Losses: games - wins,
                    Winrate: (double)wins / games,
                    AvgKda: Math.Round(avgKda, 2),
                    AvgCsm: Math.Round(avgCsm, 1),
                    AvgVisionScore: Math.Round(avgVisionScore, 0)
                );
            })
            .OrderByDescending(x => x.Games)
            .ToList();
    }

    public static List<EnemyStatsDto> ComputeEnemyStats(List<MatchEntry> matches, string role)
    {
        var adcMatches = matches.Where(m => m.Role == "ADC").ToList();
        if (adcMatches.Count == 0)
            return new List<EnemyStatsDto>();

        return adcMatches
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

    public static List<EnemyStatsDto> ComputeHardestEnemies(List<MatchEntry> matches, string? playerRole)
    {
        var filtered = !string.IsNullOrEmpty(playerRole) && playerRole != "All"
            ? matches.Where(m => m.Role == playerRole).ToList()
            : matches;

        if (filtered.Count == 0)
            return new List<EnemyStatsDto>();

        return filtered
            .Where(m => !string.IsNullOrEmpty(m.LaneEnemy))
            .GroupBy(m => m.LaneEnemy)
            .Select(g => new EnemyStatsDto(
                ChampionName: g.Key,
                Games: g.Count(),
                WinrateAgainst: g.Count(m => !m.Win) / (double)g.Count()
            ))
            .OrderByDescending(x => x.WinrateAgainst)
            .ToList();
    }

    public static List<ActivityDayDto> ComputeActivity(List<MatchEntry> matches, int months)
    {
        var defaultStart = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months));
        
        var filtered = matches.Where(m => DateOnly.FromDateTime(m.Date) >= defaultStart).ToList();
        if (filtered.Count == 0)
            return new List<ActivityDayDto>();

        return filtered
            .GroupBy(m => DateOnly.FromDateTime(m.Date))
            .Select(g => new ActivityDayDto(Date: g.Key, GamesPlayed: g.Count()))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public static EnchanterUsageSummary ComputeEnchanterUsage(List<MatchEntry> matches)
    {
        var adcMatches = matches.Where(m => m.Role == "ADC").ToList();
        if (adcMatches.Count == 0)
            return new EnchanterUsageSummary(0, 0, 0, 0, "", "");

        var mySupportGames = adcMatches.Count(m => !string.IsNullOrWhiteSpace(m.LaneAlly));
        var enemySupportGames = adcMatches.Count(m => !string.IsNullOrWhiteSpace(m.LaneEnemyAlly));

        var myEnchanterGames = adcMatches.Count(m => Enchanters.Contains(m.LaneAlly ?? ""));
        var enemyEnchanterGames = adcMatches.Count(m => Enchanters.Contains(m.LaneEnemyAlly ?? ""));

        var myTopEnchanter = adcMatches
            .Where(m => Enchanters.Contains(m.LaneAlly ?? ""))
            .GroupBy(m => m.LaneAlly)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        var enemyTopEnchanter = adcMatches
            .Where(m => Enchanters.Contains(m.LaneEnemyAlly ?? ""))
            .GroupBy(m => m.LaneEnemyAlly)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        return new EnchanterUsageSummary(
            MyEnchanterGames: myEnchanterGames,
            MyPercentage: mySupportGames == 0 ? 0 : (double)myEnchanterGames / mySupportGames * 100,
            EnemyEnchanterGames: enemyEnchanterGames,
            EnemyPercentage: enemySupportGames == 0 ? 0 : (double)enemyEnchanterGames / enemySupportGames * 100,
            MyTopEnchanter: myTopEnchanter ?? "",
            EnemyTopEnchanter: enemyTopEnchanter ?? ""
        );
    }

    public static List<DuoSummary> ComputeBestDuos(List<MatchEntry> matches, string? playerRole)
    {
        var filtered = !string.IsNullOrEmpty(playerRole) && playerRole != "All"
            ? matches.Where(m => m.Role == playerRole).ToList()
            : matches;

        if (filtered.Count == 0)
            return new List<DuoSummary>();

        return filtered
            .Where(m => !string.IsNullOrEmpty(m.LaneAlly))
            .GroupBy(m => (m.Champion, m.LaneAlly))
            .Select(g => new DuoSummary
            {
                Champion = g.Key.Champion,
                Support = g.Key.LaneAlly ?? "",
                Count = g.Count(),
                WinRate = g.Average(m => m.Win ? 1 : 0) * 100,
                AvgKda = g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths))
            })
            .Where(d => d.Count >= 2)
            .OrderByDescending(d => d.WinRate)
            .Take(3)
            .ToList();
    }

    public static List<DuoSummary> ComputeWorstEnemyDuos(List<MatchEntry> matches)
    {
        var adcMatches = matches.Where(m => m.Role == "ADC").ToList();
        if (adcMatches.Count == 0)
            return new List<DuoSummary>();

        return adcMatches
            .GroupBy(m => (m.LaneEnemy ?? "", m.LaneEnemyAlly ?? ""))
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

    public static StreakDto ComputeStreak(List<MatchEntry> matches)
    {
        if (matches.Count == 0)
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

        var ordered = matches.OrderByDescending(m => m.Date).ToList();

        // Calculate current streak
        var currentStreak = 0;
        var isWinStreak = ordered.First().Win;

        foreach (var match in ordered)
        {
            if (match.Win == isWinStreak)
                currentStreak++;
            else
                break;
        }

        // Calculate best/worst streaks
        var bestWinStreak = 0;
        var worstLossStreak = 0;
        var tempStreak = 0;
        bool? lastResult = null;

        foreach (var match in ordered.AsEnumerable().Reverse()) // Chronological order
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

    public static TimeAnalysisDto ComputeTimeAnalysis(List<MatchEntry> matches)
    {
        if (matches.Count == 0)
            return new TimeAnalysisDto();

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
                return new DayWinrateDto(DayNames[dayIndex], dayIndex, games, wins, games > 0 ? (double)wins / games : 0);
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

    public static TiltStatusDto ComputeTiltStatus(List<MatchEntry> matches)
    {
        if (matches.Count == 0)
            return new TiltStatusDto { Message = "Play some games to see your tilt status!" };

        var ordered = matches.OrderByDescending(m => m.Date).Take(10).ToList();
        var today = DateTime.Today;

        var last5 = ordered.Take(5).ToList();
        var todayMatches = ordered.Where(m => m.Date.Date == today).ToList();

        var recentLosses = 0;
        foreach (var match in ordered)
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
}
