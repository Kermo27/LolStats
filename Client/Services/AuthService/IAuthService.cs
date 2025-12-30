using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.AuthService;

public interface IAuthService
{
    Task<(bool Success, string? Error, TokenResponseDto? Token)> LoginAsync(string username, string password);
    Task<(bool Success, string? Error, TokenResponseDto? Token)> RegisterAsync(string username, string password, string? email = null);
    Task<(bool Success, TokenResponseDto? Token)> RefreshTokenAsync();
    Task LogoutAsync();
    Task<string?> GetAccessTokenAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<UserInfoDto?> GetCurrentUserAsync();
}