using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.AuthService;

public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<(bool Success, string? Error, User? User)> RegisterAsync(RegisterDto dto);
    
    /// <summary>
    /// Authenticate user and generate tokens
    /// </summary>
    Task<(bool Success, string? Error, TokenResponseDto? Token)> LoginAsync(LoginDto dto);
    
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<(bool Success, string? Error, TokenResponseDto? Token)> RefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Revoke refresh token (logout)
    /// </summary>
    Task<bool> RevokeTokenAsync(Guid userId);
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId);
    
    /// <summary>
    /// Get user ID from JWT claims
    /// </summary>
    Guid? GetUserIdFromClaims(System.Security.Claims.ClaimsPrincipal user);
}
