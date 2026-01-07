using LolStatsTracker.API.Data;
using LolStatsTracker.API.Models;
using LolStatsTracker.API.Services.AuthService;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly MatchDbContext _db;
    private readonly AuthService _service;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new MatchDbContext(options);
        
        _jwtSettings = new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };

        var jwtOptions = Options.Create(_jwtSettings);
        var logger = new Mock<ILogger<AuthService>>();
        
        _service = new AuthService(_db, jwtOptions, logger.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_ValidCredentials_CreatesUser()
    {
        var dto = new RegisterDto 
        { 
            Username = "testuser", 
            Password = "password123",
            Email = "test@example.com"
        };

        var result = await _service.RegisterAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("testuser", result.Value.Username);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsError()
    {
        _db.Users.Add(new User 
        { 
            Id = Guid.NewGuid(), 
            Username = "existinguser", 
            PasswordHash = "hash" 
        });
        await _db.SaveChangesAsync();

        var dto = new RegisterDto 
        { 
            Username = "existinguser", 
            Password = "password123" 
        };

        var result = await _service.RegisterAsync(dto);

        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Error);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsError()
    {
        _db.Users.Add(new User 
        { 
            Id = Guid.NewGuid(), 
            Username = "user1", 
            PasswordHash = "hash",
            Email = "existing@example.com"
        });
        await _db.SaveChangesAsync();

        var dto = new RegisterDto 
        { 
            Username = "newuser", 
            Password = "password123",
            Email = "existing@example.com"
        };

        var result = await _service.RegisterAsync(dto);

        Assert.True(result.IsFailure);
        Assert.Contains("Email", result.Error);
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ReturnsError()
    {
        var dto = new RegisterDto 
        { 
            Username = "testuser", 
            Password = "short" // Less than 6 characters
        };

        var result = await _service.RegisterAsync(dto);

        Assert.True(result.IsFailure);
        Assert.Contains("6 characters", result.Error);
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        var dto = new RegisterDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        };

        await _service.RegisterAsync(dto);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        Assert.NotNull(user);
        Assert.NotEqual("password123", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", user.PasswordHash));
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // First register a user
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "loginuser", 
            Password = "password123" 
        });

        var result = await _service.LoginAsync(new LoginDto 
        { 
            Username = "loginuser", 
            Password = "password123" 
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.AccessToken);
        Assert.NotEmpty(result.Value.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ReturnsError()
    {
        var result = await _service.LoginAsync(new LoginDto 
        { 
            Username = "nonexistent", 
            Password = "password123" 
        });

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid", result.Error);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsError()
    {
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "testuser", 
            Password = "correctpassword" 
        });

        var result = await _service.LoginAsync(new LoginDto 
        { 
            Username = "testuser", 
            Password = "wrongpassword" 
        });

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid", result.Error);
    }

    [Fact]
    public async Task LoginAsync_CaseInsensitiveUsername()
    {
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "TestUser", 
            Password = "password123" 
        });

        var result = await _service.LoginAsync(new LoginDto 
        { 
            Username = "TESTUSER", 
            Password = "password123" 
        });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_StoresRefreshToken()
    {
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });

        await _service.LoginAsync(new LoginDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user!.Id);
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken.Token);
        Assert.True(refreshToken.ExpiresAt > DateTime.UtcNow);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewAccessToken()
    {
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });

        var loginResult = await _service.LoginAsync(new LoginDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });

        var result = await _service.RefreshTokenAsync(loginResult.Value!.RefreshToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.AccessToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ReturnsError()
    {
        var result = await _service.RefreshTokenAsync("invalid-refresh-token");

        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_ReturnsError()
    {
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            Username = "testuser", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };
        _db.Users.Add(user);
        
        // Add expired refresh token to the new table
        var expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };
        _db.RefreshTokens.Add(expiredToken);
        await _db.SaveChangesAsync();

        var result = await _service.RefreshTokenAsync("expired-token");

        Assert.True(result.IsFailure);
        Assert.Contains("expired", result.Error);
    }

    #endregion

    #region RevokeTokenAsync Tests

    [Fact]
    public async Task RevokeTokenAsync_ValidUser_ClearsRefreshToken()
    {
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });
        
        await _service.LoginAsync(new LoginDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });

        var user = await _db.Users.FirstAsync(u => u.Username == "testuser");
        
        // Verify token exists before revoke
        var tokensBefore = await _db.RefreshTokens.CountAsync(rt => rt.UserId == user.Id);
        Assert.True(tokensBefore > 0);
        
        var result = await _service.RevokeTokenAsync(user.Id);

        Assert.True(result.IsSuccess);
        
        // Verify all tokens are removed
        var tokensAfter = await _db.RefreshTokens.CountAsync(rt => rt.UserId == user.Id);
        Assert.Equal(0, tokensAfter);
    }

    [Fact]
    public async Task RevokeTokenAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _service.RevokeTokenAsync(Guid.NewGuid());
        Assert.True(result.IsFailure);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        var registerResult = await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });

        var result = await _service.GetUserByIdAsync(registerResult.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("testuser", result.Value.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _service.GetUserByIdAsync(Guid.NewGuid());
        Assert.True(result.IsFailure);
    }

    #endregion

    public void Dispose()
    {
        _db.Dispose();
    }
}
