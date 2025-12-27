using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LolStatsTracker.Shared.Helpers;

namespace LolStatsTracker.Shared.Models;

public class MatchEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public string Champion { get; set; } = string.Empty;
    [Required] 
    public string Role { get; set; } = "ADC";
    
    /// <summary>
    /// Allied lane partner (e.g., Support for ADC, Jungler for Top/Mid, etc.)
    /// </summary>
    public string LaneAlly { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary lane opponent (e.g., enemy ADC for ADC, enemy Top for Top, etc.)
    /// </summary>
    public string LaneEnemy { get; set; } = string.Empty;
    
    /// <summary>
    /// Lane opponent's ally (e.g., enemy Support for ADC, enemy Jungler for Top/Mid, etc.)
    /// </summary>
    public string LaneEnemyAlly { get; set; } = string.Empty;
    
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    
    /// <summary>
    /// CS (Creep Score) for most roles, VisionScore for Support role.
    /// </summary>
    public int Cs { get; set; }
    
    public int GameLengthMinutes { get; set; }
    public bool Win { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string CurrentTier { get; set; } = "Unranked";
    public int CurrentDivision { get; set; } = 4;
    public int CurrentLp { get; set; }
    public string? Notes { get; set; }
    public Guid? ProfileId { get; set; }
    
    [NotMapped]
    public string KdaDisplay => $"{Kills}/{Deaths}/{Assists}";
    
    [NotMapped]
    public int PerformanceScore => PerformanceScoreHelper.Calculate(Kills, Deaths, Assists, Cs, GameLengthMinutes, Win, Role);
    
    [NotMapped]
    public string PerformanceRating => PerformanceScoreHelper.GetRating(PerformanceScore);
}