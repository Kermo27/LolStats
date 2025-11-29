namespace LolStatsTracker.Models;

public class SupportStats
{
    public string Support { get; set; } = "";
    public int Games { get; set; }
    public int Wins { get; set; }
    public double WinRate { get; set; }
    public double AvgKda { get; set; }
}