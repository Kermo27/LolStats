using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Models;

public record MatchEntry
{
    public Guid Id { get; set; }
    public DateTime? Date { get; set; } = DateTime.Now;
    public bool Win { get; set; }
    [Required(ErrorMessage = "Choose champion")]
    public string Champion { get; set; } = string.Empty;
    [Required(ErrorMessage = "Choose support")]
    public string Support { get; set; } = string.Empty;
    [Required(ErrorMessage = "Choose enemy bot")]
    public string EnemyBot { get; set; } = string.Empty;
    [Required(ErrorMessage = "Choose enemy support")]
    public string EnemySupport { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Cs { get; set; }
    public int GameLengthMinutes { get; set; }
}