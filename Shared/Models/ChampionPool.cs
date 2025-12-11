using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class ChampionPool
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public Guid ProfileId { get; set; }
    [Required]
    public string Champion { get; set; } = "";
    public string Tier { get; set; } = "Primary";
    public int Priority { get; set; } = 1;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
