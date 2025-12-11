namespace LolStatsTracker.Shared.DTOs;

public class StreakDto
{
    public bool IsWinStreak { get; set; }
    public int Count { get; set; }
    public int BestWinStreak { get; set; }
    public int WorstLossStreak { get; set; }
    public int TotalGames { get; set; }
}
