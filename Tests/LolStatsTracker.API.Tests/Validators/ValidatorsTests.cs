using LolStatsTracker.API.Validators;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Xunit;

namespace LolStatsTracker.API.Tests.Validators;

public class AuthValidatorsTests
{
    private readonly RegisterDtoValidator _registerValidator = new();
    private readonly LoginDtoValidator _loginValidator = new();

    [Theory]
    [InlineData("ab", false)] // Too short
    [InlineData("abc", true)]
    [InlineData("validuser", true)]
    [InlineData("user_name123", true)]
    [InlineData("invalid user", false)] // Contains space
    [InlineData("user@name", false)] // Contains @
    public void RegisterDto_Username_Validation(string username, bool shouldPass)
    {
        // Arrange
        var dto = new RegisterDto
        {
            Username = username,
            Password = "ValidPass123!",
            Email = "test@example.com"
        };

        // Act
        var result = _registerValidator.Validate(dto);
        var hasUsernameError = result.Errors.Any(e => e.PropertyName == nameof(RegisterDto.Username));

        // Assert
        Assert.Equal(shouldPass, !hasUsernameError);
    }

    [Theory]
    [InlineData("short", false)] // Too short
    [InlineData("nouppercase1", false)] // No uppercase
    [InlineData("NOLOWERCASE1", false)] // No lowercase
    [InlineData("NoNumbers", false)] // No numbers
    [InlineData("ValidPass1", true)]
    [InlineData("MySecureP@ss123", true)]
    public void RegisterDto_Password_Validation(string password, bool shouldPass)
    {
        // Arrange
        var dto = new RegisterDto
        {
            Username = "validuser",
            Password = password,
            Email = "test@example.com"
        };

        // Act
        var result = _registerValidator.Validate(dto);
        var hasPasswordError = result.Errors.Any(e => e.PropertyName == nameof(RegisterDto.Password));

        // Assert
        Assert.Equal(shouldPass, !hasPasswordError);
    }

    [Theory]
    [InlineData("valid@email.com", true)]
    [InlineData("user.name@domain.org", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", true)] // Email is optional
    public void RegisterDto_Email_Validation(string email, bool shouldPass)
    {
        // Arrange
        var dto = new RegisterDto
        {
            Username = "validuser",
            Password = "ValidPass123",
            Email = email
        };

        // Act
        var result = _registerValidator.Validate(dto);
        var hasEmailError = result.Errors.Any(e => e.PropertyName == nameof(RegisterDto.Email));

        // Assert
        Assert.Equal(shouldPass, !hasEmailError);
    }

    [Fact]
    public void LoginDto_EmptyCredentials_HasErrors()
    {
        // Arrange
        var dto = new LoginDto { Username = "", Password = "" };

        // Act
        var result = _loginValidator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginDto.Username));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginDto.Password));
    }

    [Fact]
    public void LoginDto_ValidCredentials_NoErrors()
    {
        // Arrange
        var dto = new LoginDto { Username = "testuser", Password = "password123" };

        // Act
        var result = _loginValidator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }
}

public class MatchEntryValidatorTests
{
    private readonly MatchEntryValidator _validator = new();

    [Fact]
    public void ValidMatch_NoErrors()
    {
        // Arrange
        var match = CreateValidMatch();

        // Act
        var result = _validator.Validate(match);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Kills_OutOfRange_HasError(int kills)
    {
        // Arrange
        var match = CreateValidMatch();
        match.Kills = kills;

        // Act
        var result = _validator.Validate(match);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MatchEntry.Kills));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Deaths_OutOfRange_HasError(int deaths)
    {
        // Arrange
        var match = CreateValidMatch();
        match.Deaths = deaths;

        // Act
        var result = _validator.Validate(match);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MatchEntry.Deaths));
    }

    [Theory]
    [InlineData("BotLane")]
    [InlineData("Marksman")]
    public void Role_Invalid_HasError(string role)
    {
        // Arrange
        var match = CreateValidMatch();
        match.Role = role;

        // Act
        var result = _validator.Validate(match);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MatchEntry.Role));
    }

    [Theory]
    [InlineData("Top")]
    [InlineData("Jungle")]
    [InlineData("Mid")]
    [InlineData("ADC")]
    [InlineData("Support")]
    public void Role_Valid_NoError(string role)
    {
        // Arrange
        var match = CreateValidMatch();
        match.Role = role;

        // Act
        var result = _validator.Validate(match);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(MatchEntry.Role));
    }

    [Fact]
    public void GameLength_TooLong_HasError()
    {
        // Arrange
        var match = CreateValidMatch();
        match.GameLengthMinutes = 200;

        // Act
        var result = _validator.Validate(match);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MatchEntry.GameLengthMinutes));
    }

    private static MatchEntry CreateValidMatch()
    {
        return new MatchEntry
        {
            Id = Guid.NewGuid(),
            Champion = "Jinx",
            Role = "ADC",
            LaneAlly = "Thresh",
            LaneEnemy = "Caitlyn",
            LaneEnemyAlly = "Lux",
            Kills = 10,
            Deaths = 3,
            Assists = 8,
            Cs = 220,
            GameLengthMinutes = 32,
            Win = true,
            Date = DateTime.UtcNow,
            CurrentTier = "Gold",
            CurrentDivision = 2,
            CurrentLp = 45,
            GameMode = "Ranked Solo",
            QueueId = 420
        };
    }
}
