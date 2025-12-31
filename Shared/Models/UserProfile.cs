using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to the authenticated user (nullable for legacy profiles)
    /// </summary>
    public Guid? UserId { get; set; }
    
    [Required]
    public string Name { get; set; } = "Main Account";
    public string Tag { get; set; } = "EUW";
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Riot account PUUID for linking to Riot API data
    /// </summary>
    public string? RiotPuuid { get; set; }
    
    /// <summary>
    /// Profile icon ID from Riot API (for displaying summoner icon)
    /// </summary>
    public int? ProfileIconId { get; set; }
    
    /// <summary>
    /// Current Solo/Duo ranked tier (e.g., "DIAMOND", "GOLD")
    /// </summary>
    public string? SoloTier { get; set; }
    
    /// <summary>
    /// Current Solo/Duo ranked division (e.g., "IV", "II")
    /// </summary>
    public string? SoloRank { get; set; }
    
    /// <summary>
    /// Current Solo/Duo League Points
    /// </summary>
    public int? SoloLP { get; set; }
}