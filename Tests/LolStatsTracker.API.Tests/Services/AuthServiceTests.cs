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

        var (success, error, user) = await _service.RegisterAsync(dto);

        Assert.True(success);
        Assert.Null(error);
        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
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

        var (success, error, user) = await _service.RegisterAsync(dto);

        Assert.False(success);
        Assert.Contains("already exists", error);
        Assert.Null(user);
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

        var (success, error, user) = await _service.RegisterAsync(dto);

        Assert.False(success);
        Assert.Contains("Email", error);
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ReturnsError()
    {
        var dto = new RegisterDto 
        { 
            Username = "testuser", 
            Password = "short" // Less than 6 characters
        };

        var (success, error, user) = await _service.RegisterAsync(dto);

        Assert.False(success);
        Assert.Contains("6 characters", error);
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

        var (success, error, token) = await _service.LoginAsync(new LoginDto 
        { 
            Username = "loginuser", 
            Password = "password123" 
        });

        Assert.True(success);
        Assert.Null(error);
        Assert.NotNull(token);
        Assert.NotEmpty(token.AccessToken);
        Assert.NotEmpty(token.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ReturnsError()
    {
        var (success, error, token) = await _service.LoginAsync(new LoginDto 
        { 
            Username = "nonexistent", 
            Password = "password123" 
        });

        Assert.False(success);
        Assert.Contains("Invalid", error);
        Assert.Null(token);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsError()
    {
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "testuser", 
            Password = "correctpassword" 
        });

        var (success, error, token) = await _service.LoginAsync(new LoginDto 
        { 
            Username = "testuser", 
            Password = "wrongpassword" 
        });

        Assert.False(success);
        Assert.Contains("Invalid", error);
    }

    [Fact]
    public async Task LoginAsync_CaseInsensitiveUsername()
    {
        await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "TestUser", 
            Password = "password123" 
        });

        var (success, error, token) = await _service.LoginAsync(new LoginDto 
        { 
            Username = "TESTUSER", 
            Password = "password123" 
        });

        Assert.True(success);
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
        Assert.NotNull(user?.RefreshToken);
        Assert.NotNull(user?.RefreshTokenExpiry);
        Assert.True(user.RefreshTokenExpiry > DateTime.UtcNow);
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

        var (success, error, newToken) = await _service.RefreshTokenAsync(loginResult.Token!.RefreshToken);

        Assert.True(success);
        Assert.NotNull(newToken);
        Assert.NotEmpty(newToken.AccessToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ReturnsError()
    {
        var (success, error, token) = await _service.RefreshTokenAsync("invalid-refresh-token");

        Assert.False(success);
        Assert.Null(token);
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_ReturnsError()
    {
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            Username = "testuser", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            RefreshToken = "expired-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1) // Expired
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (success, error, token) = await _service.RefreshTokenAsync("expired-token");

        Assert.False(success);
        Assert.Contains("expired", error);
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
        
        var result = await _service.RevokeTokenAsync(user.Id);

        Assert.True(result);
        
        await _db.Entry(user).ReloadAsync();
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiry);
    }

    [Fact]
    public async Task RevokeTokenAsync_NonExistentUser_ReturnsFalse()
    {
        var result = await _service.RevokeTokenAsync(Guid.NewGuid());
        Assert.False(result);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        var (_, _, user) = await _service.RegisterAsync(new RegisterDto 
        { 
            Username = "testuser", 
            Password = "password123" 
        });

        var result = await _service.GetUserByIdAsync(user!.Id);

        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ReturnsNull()
    {
        var result = await _service.GetUserByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    #endregion

    public void Dispose()
    {
        _db.Dispose();
    }
}
