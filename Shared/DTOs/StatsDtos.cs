namespace LolStatsTracker.Shared.DTOs;

public record StatsSummaryDto(
    OverviewDto Overview,
    List<ChampionStatsDto> ChampionStats,
    List<EnemyStatsDto> EnemyBotStats,
    List<EnemyStatsDto> EnemySupportStats,
    List<ActivityDayDto> Activity,
    EnchanterUsageSummary EnchanterUsage,
    List<DuoSummary> BestDuos,
    List<DuoSummary> WorstEnemyDuos,
    StreakDto Streak,
    TimeAnalysisDto TimeAnalysis,
    TiltStatusDto TiltStatus
);

public record OverviewDto(double Winrate, string MostPlayedChampion, int MostPlayedChampionGames, string FavoriteSupport, int FavoriteSupportGames);
public record ChampionStatsDto(string ChampionName, string Role, int Games, int Wins, int Losses, double Winrate);
public record EnemyStatsDto(string ChampionName, int Games, double WinrateAgainst);
public record ActivityDayDto(DateOnly Date, int GamesPlayed);
public record EnchanterUsageSummary(int MyEnchanterGames, double MyPercentage, int EnemyEnchanterGames, double EnemyPercentage, string MyTopEnchanter = "", string EnemyTopEnchanter = "");
public record DuoSummary {
    public string Champion { get; set; } = "";
    public string Support { get; set; } = "";
    public int Count { get; set; }
    public double WinRate { get; set; }
    public double AvgKda { get; set; }
}