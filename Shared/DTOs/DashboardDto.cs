namespace LolStatsTracker.Shared.DTOs;

public class DashboardDto
{
    public int TotalGames { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double Winrate { get; set; }
    public double Kda { get; set; }
    public int CurrentStreak { get; set; }
    public List<ChampionStatsDto> TopChampion { get; set; } = new();
    public List<int> RecentHistory { get; set; } = new();
}