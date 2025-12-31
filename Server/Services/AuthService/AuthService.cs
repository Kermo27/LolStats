using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LolStatsTracker.API.Data;
using LolStatsTracker.API.Models;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LolStatsTracker.API.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly MatchDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        MatchDbContext context,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, User? User)> RegisterAsync(RegisterDto dto)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

            if (existingUser != null)
            {
                return (false, "Username already exists", null);
            }

            // Check if email already exists (if provided)
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == dto.Email.ToLower());

                if (existingEmail != null)
                {
                    return (false, "Email already registered", null);
                }
            }

            // Validate password strength
            if (dto.Password.Length < 6)
            {
                return (false, "Password must be at least 6 characters", null);
            }

            // Create user with BCrypt hashed password
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered: {Username}", user.Username);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Username}", dto.Username);
            return (false, "An error occurred during registration", null);
        }
    }

    public async Task<(bool Success, string? Error, TokenResponseDto? Token)> LoginAsync(LoginDto dto)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

            if (user == null)
            {
                return (false, "Invalid username or password", null);
            }

            // Verify password with BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return (false, "Invalid username or password", null);
            }

            // Generate tokens
            var tokenResponse = await GenerateTokensAsync(user);

            _logger.LogInformation("User logged in: {Username}", user.Username);
            return (true, null, tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", dto.Username);
            return (false, "An error occurred during login", null);
        }
    }

    public async Task<(bool Success, string? Error, TokenResponseDto? Token)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                _logger.LogWarning("Refresh failed: Invalid token provided");
                return (false, "Refresh token not found or already rotated", null);
            }

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh failed: Token expired for user {Username}", user.Username);
                // Clear expired refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();
                return (false, "Refresh token expired", null);
            }

            // Generate NEW access token but KEEP the same refresh token (no rotation)
            var accessToken = GenerateAccessToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

            _logger.LogInformation("Token refreshed for user: {Username}", user.Username);
            
            return (true, null, new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken, // Return the same refresh token
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return (false, "An error occurred during token refresh", null);
        }
    }

    public async Task<bool> RevokeTokenAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Token revoked for user: {Username}", user.Username);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public Guid? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private async Task<TokenResponseDto> GenerateTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token to database
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            }
        };
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
