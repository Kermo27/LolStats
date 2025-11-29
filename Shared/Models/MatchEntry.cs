using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class MatchEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public string Champion { get; set; } = string.Empty;
    public string Support { get; set; } = string.Empty;
    public string EnemyBot { get; set; } = string.Empty;
    public string EnemySupport { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Cs { get; set; }
    public int GameLengthMinutes { get; set; }
    public bool Win { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
}