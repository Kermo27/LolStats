using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Email { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    public string Role { get; set; } = "User";

    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiry { get; set; }
}
