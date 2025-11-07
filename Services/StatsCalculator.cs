using LolStatsTracker.Models;

namespace LolStatsTracker.Services;

public class StatsCalculator
{
    private readonly IEnumerable<MatchEntry> _matches;

    public StatsCalculator(IEnumerable<MatchEntry> matches)
    {
        _matches = matches;
    }

    public StatsSummary GetSummary()
    {
        if (!_matches.Any())
            return new StatsSummary(new(), new(), new(), 0);

        // Średni CSM (creep score per minute)
        var avgCsm = _matches
            .Where(m => m.GameLengthMinutes > 0)
            .Average(m => (double)m.Cs / m.GameLengthMinutes);

        // --- CHAMPIONS ---
        var champGroups = _matches
            .GroupBy(m => m.Champion)
            .Select(g => new ChampionSummary(
                g.Key,
                g.Count(),
                g.Average(m => m.Win ? 1 : 0) * 100,
                g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths))
            ))
            .ToList();

        var avgChampCount = champGroups.Average(x => x.Count);

        var topChampions = champGroups
            .Where(x => x.Count >= avgChampCount)
            .OrderByDescending(x => x.WinRate)
            .ThenByDescending(x => x.AvgKda)
            .Take(3)
            .ToList();

        // --- SUPPORTS ---
        var supportGroups = _matches
            .GroupBy(m => m.Support)
            .Select(g => new SupportSummary(
                g.Key,
                g.Count(),
                g.Average(m => m.Win ? 1 : 0) * 100
            ))
            .ToList();

        var avgSupportCount = supportGroups.Average(x => x.Count);

        var topSupports = supportGroups
            .Where(x => x.Count >= avgSupportCount)
            .OrderByDescending(x => x.WinRate)
            .Take(3)
            .ToList();

        // --- ENEMY BOTLANES ---
        var enemyGroups = _matches
            .GroupBy(m => $"{m.EnemyBot} + {m.EnemySupport}")
            .Select(g => new EnemyBotlaneSummary(
                g.Key,
                g.Count(),
                g.Average(m => m.Win ? 1 : 0) * 100
            ))
            .ToList();

        var avgEnemyCount = enemyGroups.Average(x => x.Count);

        var worstEnemies = enemyGroups
            .Where(x => x.Count >= avgEnemyCount)
            .OrderBy(x => x.WinRate)
            .Take(3)
            .ToList();

        return new StatsSummary(topChampions, topSupports, worstEnemies, avgCsm);
    }
}
