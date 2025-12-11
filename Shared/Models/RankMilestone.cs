using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class RankMilestone
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid ProfileId { get; set; }
    public string Tier { get; set; } = "";
    public int Division { get; set; }
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    public Guid? MatchId { get; set; }
    public string Type { get; set; } = "Promotion";
}
