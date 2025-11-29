namespace LolStatsTracker.Models;

public class MatchStats
{
    public string Champion { get; set; } = string.Empty;
    public int Games { get; set; }
    public int Wins { get; set; }
    public double WinRate { get; set; }
    public double AvgKda { get; set; }
    public double AvgCsm { get; set; }
}