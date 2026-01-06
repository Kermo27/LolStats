using LolStatsTracker.API.Services.AuthService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class LoginAttemptServiceTests
{
    private readonly LoginAttemptService _service;
    private readonly Mock<ILogger<LoginAttemptService>> _loggerMock;

    public LoginAttemptServiceTests()
    {
        _loggerMock = new Mock<ILogger<LoginAttemptService>>();
        _service = new LoginAttemptService(_loggerMock.Object);
    }

    [Fact]
    public async Task IsLockedOut_NewUser_ReturnsFalse()
    {
        // Act
        var result = await _service.IsLockedOutAsync("newuser");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetRemainingAttempts_NewUser_ReturnsMaxAttempts()
    {
        // Act
        var remaining = await _service.GetRemainingAttemptsAsync("newuser");

        // Assert
        Assert.Equal(5, remaining);
    }

    [Fact]
    public async Task RecordFailedAttempt_DecrementsRemainingAttempts()
    {
        // Arrange
        var username = "testuser1";

        // Act
        await _service.RecordFailedAttemptAsync(username);
        var remaining = await _service.GetRemainingAttemptsAsync(username);

        // Assert
        Assert.Equal(4, remaining);
    }

    [Fact]
    public async Task RecordFailedAttempt_FiveAttempts_LocksAccount()
    {
        // Arrange
        var username = "testuser2";

        // Act - 5 failed attempts
        for (int i = 0; i < 5; i++)
        {
            await _service.RecordFailedAttemptAsync(username);
        }

        var isLocked = await _service.IsLockedOutAsync(username);
        var remaining = await _service.GetRemainingAttemptsAsync(username);

        // Assert
        Assert.True(isLocked);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task ClearAttempts_ResetsLockout()
    {
        // Arrange
        var username = "testuser3";
        
        // Lock the account first
        for (int i = 0; i < 5; i++)
        {
            await _service.RecordFailedAttemptAsync(username);
        }
        Assert.True(await _service.IsLockedOutAsync(username));

        // Act - clear attempts
        await _service.ClearAttemptsAsync(username);

        // Assert
        Assert.False(await _service.IsLockedOutAsync(username));
        Assert.Equal(5, await _service.GetRemainingAttemptsAsync(username));
    }

    [Fact]
    public async Task IsLockedOut_CaseInsensitive()
    {
        // Arrange
        var username = "TestUser4";

        // Lock with lowercase
        for (int i = 0; i < 5; i++)
        {
            await _service.RecordFailedAttemptAsync(username.ToLower());
        }

        // Act - check with original case
        var isLocked = await _service.IsLockedOutAsync(username);

        // Assert
        Assert.True(isLocked);
    }
}
