namespace LolStatsTracker.Models;

public record BasicSummary(string Name, int Count, double WinRate);
public record EnchanterCardItem(string Name, string Desc);
public class DuoSummary
{
    public string Champion { get; set; } = string.Empty;
    public string Support { get; set; } = string.Empty;
    public int Count { get; set; }
    public double WinRate { get; set; }
    public double AvgKda { get; set; }
    
    public string Name => $"{Champion} + {Support}";
}

public record ChampionSummary(
    string Name,
    int Count,
    double WinRate,
    double AvgKda
) : BasicSummary(Name, Count, WinRate);

public record StatsSummary(
    List<ChampionSummary> TopChampions,
    List<BasicSummary> TopSupports,
    List<BasicSummary> WorstEnemyBots,
    List<BasicSummary> WorstEnemySupports,
    double AverageCsPerMinute,
    int TotalMatches,
    DateTime GeneratedAt,
    EnchanterUsageSummary EnchanterUsage,
    List<DuoSummary> BestDuos,
    List<DuoSummary> WorstEnemyDuos
);

public record EnchanterUsageSummary(
    int MyEnchanterGames,
    double MyEnchanterPercentage,
    int EnemyEnchanterGames,
    double EnemyEnchanterPercentage
);
