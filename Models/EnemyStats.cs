namespace LolStatsTracker.Models;

public class EnemyStats
{
    public string Enemy { get; set; } = "";
    public int Games { get; set; }
    public int Wins { get; set; }
    public double WinRate { get; set; }
}