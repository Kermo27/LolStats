using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    
    public Guid? UserId { get; set; }
    
    [Required]
    public string Name { get; set; } = "Main Account";
    public string Tag { get; set; } = "EUW";
    public bool IsDefault { get; set; }

    public string? RiotPuuid { get; set; }
    
    public int? ProfileIconId { get; set; }

    public string? SoloTier { get; set; }

    public string? SoloRank { get; set; }

    public int? SoloLP { get; set; }

    public string? FlexTier { get; set; }

    public string? FlexRank { get; set; }

    public int? FlexLP { get; set; }
}