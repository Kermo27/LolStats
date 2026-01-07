using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string? DeviceInfo { get; set; }
    public string? UserAgent { get; set; }
    public User User { get; set; } = null!;
}
