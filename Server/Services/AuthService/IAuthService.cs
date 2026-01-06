using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.AuthService;

public interface IAuthService
{
    Task<Result<User>> RegisterAsync(RegisterDto dto);
    Task<Result<TokenResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<Result> RevokeTokenAsync(Guid userId);
    Task<Result<User>> GetUserByIdAsync(Guid userId);
    Guid? GetUserIdFromClaims(System.Security.Claims.ClaimsPrincipal user);
}
