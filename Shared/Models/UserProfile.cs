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
}