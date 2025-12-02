using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = "Main Account";
    public string Tag { get; set; } = "EUW";
    public bool IsDefault { get; set; }
}