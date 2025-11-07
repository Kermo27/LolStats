namespace LolStatsTracker.Models;

public record ChampionSummary(string Name, int Count, double WinRate, double AvgKda);
public record SupportSummary(string Name, int Count, double WinRate);
public record EnemyBotlaneSummary(string Name, int Count, double WinRate);

public record StatsSummary(
    List<ChampionSummary> TopChampions,
    List<SupportSummary> TopSupports,
    List<EnemyBotlaneSummary> WorstEnemies,
    double AverageCsm
);