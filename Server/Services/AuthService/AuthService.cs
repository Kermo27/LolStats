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

    public async Task<Result<User>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

            if (existingUser != null)
            {
                return Result<User>.Failure("Username already exists", ErrorCodes.Conflict);
            }

            // Check if email already exists (if provided)
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == dto.Email.ToLower());

                if (existingEmail != null)
                {
                    return Result<User>.Failure("Email already registered", ErrorCodes.Conflict);
                }
            }

            // Validate password strength
            if (dto.Password.Length < 6)
            {
                return Result<User>.ValidationError("Password must be at least 6 characters");
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
            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Username}", dto.Username);
            return Result<User>.Failure("An error occurred during registration");
        }
    }

    public async Task<Result<TokenResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

            if (user == null)
            {
                return Result<TokenResponseDto>.Unauthorized("Invalid username or password");
            }

            // Verify password with BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Result<TokenResponseDto>.Unauthorized("Invalid username or password");
            }

            // Generate tokens
            var tokenResponse = await GenerateTokensAsync(user);

            _logger.LogInformation("User logged in: {Username}", user.Username);
            return Result<TokenResponseDto>.Success(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", dto.Username);
            return Result<TokenResponseDto>.Failure("An error occurred during login");
        }
    }

    public async Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                _logger.LogWarning("Refresh failed: Invalid token provided");
                return Result<TokenResponseDto>.Unauthorized("Refresh token not found or already rotated");
            }

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh failed: Token expired for user {Username}", user.Username);
                // Clear expired refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();
                return Result<TokenResponseDto>.Unauthorized("Refresh token expired");
            }

            // Generate NEW access token but KEEP the same refresh token (no rotation)
            var accessToken = GenerateAccessToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

            _logger.LogInformation("Token refreshed for user: {Username}", user.Username);
            
            return Result<TokenResponseDto>.Success(new TokenResponseDto
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
            return Result<TokenResponseDto>.Failure("An error occurred during token refresh");
        }
    }

    public async Task<Result> RevokeTokenAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Result.NotFound("User not found");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Token revoked for user: {Username}", user.Username);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token for user: {UserId}", userId);
            return Result.Failure("An error occurred while revoking token");
        }
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return Result<User>.NotFound("User not found");
        }
        return Result<User>.Success(user);
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
