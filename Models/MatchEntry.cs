namespace LolStatsTracker.Models;

public class MatchEntry
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public bool Win { get; set; }
    public string Champion { get; set; } = string.Empty;
    public string Support { get; set; } = string.Empty;
    public string EnemyBot { get; set; } = string.Empty;
    public string EnemySupport { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Cs { get; set; }
    public int GameLengthMinutes { get; set; }
}