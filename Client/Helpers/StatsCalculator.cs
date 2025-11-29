using LolStatsTracker.Models;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Helpers;

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
        {
            return new StatsSummary(
                TopChampions: new(),
                TopSupports: new(),
                WorstEnemyBots: new(),
                WorstEnemySupports: new(),
                AverageCsPerMinute: 0,
                TotalMatches: 0,
                GeneratedAt: DateTime.Now,
                EnchanterUsage: CalculateEnchanterUsage(),
                BestDuos: new(),
                WorstEnemyDuos: new()
            );
        }

        return new StatsSummary(
            TopChampions: CalculateTopChampions(),
            TopSupports: CalculateTopSupports(),
            WorstEnemyBots: CalculateWorstEnemyBots(),
            WorstEnemySupports: CalculateWorstEnemySupports(),
            AverageCsPerMinute: CalculateAverageCsPerMinute(),
            TotalMatches: _matches.Count(),
            GeneratedAt: DateTime.Now,
            EnchanterUsage: CalculateEnchanterUsage(),
            BestDuos: GetBestDuos(),
            WorstEnemyDuos: GetWorstEnemyDuos()
        );
    }
    
    private double CalculateAverageCsPerMinute()
    {
        return _matches
            .Where(m => m.GameLengthMinutes > 0)
            .Average(m => (double)m.Cs / m.GameLengthMinutes);
    }
    
    private List<ChampionSummary> CalculateTopChampions()
    {
        var groups = _matches
            .GroupBy(m => m.Champion)
            .Select(g => new ChampionSummary(
                g.Key,
                g.Count(),
                g.Average(m => m.Win ? 1 : 0) * 100,
                g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths))
            ))
            .ToList();

        var avg = groups.Average(x => x.Count);

        return groups
            .Where(x => x.Count >= avg)
            .OrderByDescending(x => x.WinRate)
            .ThenByDescending(x => x.AvgKda)
            .Take(3)
            .ToList();
    }
    
    private List<BasicSummary> CalculateTopSupports()
    {
        var groups = _matches
            .GroupBy(m => m.Support)
            .Select(g => new BasicSummary(
                g.Key,
                g.Count(),
                g.Average(m => m.Win ? 1 : 0) * 100
            ))
            .ToList();

        var avg = groups.Average(x => x.Count);

        return groups
            .Where(x => x.Count >= avg)
            .OrderByDescending(x => x.WinRate)
            .Take(3)
            .ToList();
    }
    
    private List<BasicSummary> CalculateWorstEnemyBots()
    {
        var groups = _matches
            .GroupBy(m => m.EnemyBot)
            .Select(g => new BasicSummary(
                g.Key,
                g.Count(),
                g.Average(m => m.Win ? 1 : 0) * 100
            ))
            .ToList();

        var avg = groups.Average(x => x.Count);

        return groups
            .Where(x => x.Count >= avg)
            .OrderBy(x => x.WinRate)
            .Take(3)
            .ToList();
    }
    
    private List<BasicSummary> CalculateWorstEnemySupports()
    {
        var groups = _matches
            .GroupBy(m => m.EnemySupport)
            .Select(g => new BasicSummary(
                g.Key,
                g.Count(),
                g.Average(m => m.Win ? 1 : 0) * 100
            ))
            .ToList();

        var avg = groups.Average(x => x.Count);

        return groups
            .Where(x => x.Count >= avg)
            .OrderBy(x => x.WinRate)
            .Take(3)
            .ToList();
    }
    
    private EnchanterUsageSummary CalculateEnchanterUsage()
    {
        var enchanters = ChampionList.Enchanters;

        var mySupportGames = _matches.Count(m => !string.IsNullOrWhiteSpace(m.Support));
        var enemySupportGames = _matches.Count(m => !string.IsNullOrWhiteSpace(m.EnemySupport));

        var myEnchanterGames = _matches.Count(m => enchanters.Contains(m.Support));
        var enemyEnchanterGames = _matches.Count(m => enchanters.Contains(m.EnemySupport));

        var myPerc = mySupportGames == 0 ? 0 : (double)myEnchanterGames / mySupportGames * 100;
        var enemyPerc = enemySupportGames == 0 ? 0 : (double)enemyEnchanterGames / enemySupportGames * 100;

        return new EnchanterUsageSummary(
            myEnchanterGames,
            myPerc,
            enemyEnchanterGames,
            enemyPerc
        );
    }
    
    private List<DuoSummary> GetBestDuos()
    {
        return _matches
            .GroupBy(m => (m.Champion, m.Support))
            .Select(g => new DuoSummary
            {
                Champion = g.Key.Item1,
                Support = g.Key.Item2,
                Count = g.Count(),
                WinRate = g.Average(m => m.Win ? 1 : 0) * 100,
                AvgKda = g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths))
            })
            .Where(d => d.Count >= 3)
            .OrderByDescending(d => d.WinRate)
            .Take(3)
            .ToList();

    }

    private List<DuoSummary> GetWorstEnemyDuos()
    {
        return _matches
            .GroupBy(m => (m.EnemyBot, m.EnemySupport))
            .Select(g => new DuoSummary
            {
                Champion = g.Key.Item1,
                Support = g.Key.Item2,
                Count = g.Count(),
                WinRate = g.Average(m => m.Win ? 1 : 0) * 100,
                AvgKda = g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths))
            })
            .Where(d => d.Count >= 3)
            .OrderBy(d => d.WinRate)
            .Take(3)
            .ToList();
    }
}
