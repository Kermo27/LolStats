using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

/// <summary>
/// Represents an authenticated user in the system
/// </summary>
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
    
    /// <summary>
    /// User role for authorization (User, Admin)
    /// </summary>
    [MaxLength(20)]
    public string Role { get; set; } = "User";
    
    /// <summary>
    /// Refresh token for JWT refresh flow
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Expiry date for the refresh token
    /// </summary>
    public DateTime? RefreshTokenExpiry { get; set; }
}
