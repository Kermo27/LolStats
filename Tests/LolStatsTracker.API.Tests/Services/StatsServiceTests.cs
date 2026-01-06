using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.CacheService;
using LolStatsTracker.API.Services.StatsService;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class StatsServiceTests : IDisposable
{
    private readonly MatchDbContext _db;
    private readonly StatsService _service;
    private readonly Guid _profileId = Guid.NewGuid();
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<StatsService>> _loggerMock;

    public StatsServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new MatchDbContext(options);
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<StatsService>>();
        _service = new StatsService(_db, _cacheMock.Object, _loggerMock.Object);

        _db.UserProfiles.Add(new UserProfile { Id = _profileId, Name = "TestUser" });
        _db.SaveChanges();
    }

    #region GetOverviewAsync Tests

    [Fact]
    public async Task GetOverviewAsync_NoMatches_ReturnsEmptyOverview()
    {
        var result = await _service.GetOverviewAsync(_profileId);

        Assert.Equal(0, result.Winrate);
        Assert.Empty(result.MostPlayedChampion);
    }

    [Fact]
    public async Task GetOverviewAsync_WithMatches_CalculatesWinrate()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = false },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = false }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetOverviewAsync(_profileId);

        Assert.Equal(0.5, result.Winrate);
    }

    [Fact]
    public async Task GetOverviewAsync_FindsMostPlayedChampion()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Win = true }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetOverviewAsync(_profileId);

        Assert.Equal("Jinx", result.MostPlayedChampion);
        Assert.Equal(2, result.MostPlayedChampionGames);
    }

    [Fact]
    public async Task GetOverviewAsync_FindsFavoriteSupport()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Role = "ADC", LaneAlly = "Lulu", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Role = "ADC", LaneAlly = "Lulu", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Role = "ADC", LaneAlly = "Thresh", Win = true }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetOverviewAsync(_profileId);

        Assert.Equal("Lulu", result.FavoriteSupport);
        Assert.Equal(2, result.FavoriteSupportGames);
    }

    #endregion

    #region GetChampionStatsAsync Tests

    [Fact]
    public async Task GetChampionStatsAsync_NoMatches_ReturnsEmptyList()
    {
        var result = await _service.GetChampionStatsAsync(_profileId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChampionStatsAsync_GroupsByChampion()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = false },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Win = true }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetChampionStatsAsync(_profileId);

        Assert.Equal(2, result.Count);
        
        var jinx = result.First(c => c.ChampionName == "Jinx");
        Assert.Equal(2, jinx.Games);
        Assert.Equal(1, jinx.Wins);
        Assert.Equal(0.5, jinx.Winrate);
    }

    [Fact]
    public async Task GetChampionStatsAsync_OrdersByGamesDescending()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Win = true }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetChampionStatsAsync(_profileId);

        Assert.Equal("Jinx", result[0].ChampionName);
    }

    #endregion

    #region GetStreakAsync Tests

    [Fact]
    public async Task GetStreakAsync_NoMatches_ReturnsZeroStreak()
    {
        var result = await _service.GetStreakAsync(_profileId);

        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.TotalGames);
    }

    [Fact]
    public async Task GetStreakAsync_WinStreak_CalculatesCorrectly()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true, Date = DateTime.UtcNow },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true, Date = DateTime.UtcNow.AddMinutes(-30) },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true, Date = DateTime.UtcNow.AddMinutes(-60) },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = false, Date = DateTime.UtcNow.AddMinutes(-90) }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetStreakAsync(_profileId);

        Assert.True(result.IsWinStreak);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetStreakAsync_LossStreak_CalculatesCorrectly()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = false, Date = DateTime.UtcNow },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = false, Date = DateTime.UtcNow.AddMinutes(-30) },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true, Date = DateTime.UtcNow.AddMinutes(-60) }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetStreakAsync(_profileId);

        Assert.False(result.IsWinStreak);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetStreakAsync_CalculatesBestAndWorstStreaks()
    {
        _db.Matches.AddRange(
            // Recent: 2 wins
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow },
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow.AddHours(-1) },
            // Then: 3 losses
            new MatchEntry { ProfileId = _profileId, Win = false, Date = DateTime.UtcNow.AddHours(-2) },
            new MatchEntry { ProfileId = _profileId, Win = false, Date = DateTime.UtcNow.AddHours(-3) },
            new MatchEntry { ProfileId = _profileId, Win = false, Date = DateTime.UtcNow.AddHours(-4) },
            // Before: 4 wins
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow.AddHours(-5) },
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow.AddHours(-6) },
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow.AddHours(-7) },
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow.AddHours(-8) }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetStreakAsync(_profileId);

        Assert.Equal(4, result.BestWinStreak);
        Assert.Equal(3, result.WorstLossStreak);
    }

    #endregion

    #region GetTiltStatusAsync Tests

    [Fact]
    public async Task GetTiltStatusAsync_NoMatches_ReturnsDefaultMessage()
    {
        var result = await _service.GetTiltStatusAsync(_profileId);

        Assert.Contains("Play some games", result.Message);
    }

    [Fact]
    public async Task GetTiltStatusAsync_WinStreak_NotTilted()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow },
            new MatchEntry { ProfileId = _profileId, Win = true, Date = DateTime.UtcNow.AddMinutes(-30) }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetTiltStatusAsync(_profileId);

        Assert.False(result.IsTilted);
        Assert.Contains("playing well", result.Message);
    }

    [Fact]
    public async Task GetTiltStatusAsync_ManyLosses_IsTilted()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Win = false, Date = DateTime.UtcNow },
            new MatchEntry { ProfileId = _profileId, Win = false, Date = DateTime.UtcNow.AddMinutes(-30) },
            new MatchEntry { ProfileId = _profileId, Win = false, Date = DateTime.UtcNow.AddMinutes(-60) },
            new MatchEntry { ProfileId = _profileId, Win = false, Date = DateTime.UtcNow.AddMinutes(-90) }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetTiltStatusAsync(_profileId);

        Assert.True(result.IsTilted);
    }

    #endregion

    #region GetTimeAnalysisAsync Tests

    [Fact]
    public async Task GetTimeAnalysisAsync_NoMatches_ReturnsEmptyAnalysis()
    {
        var result = await _service.GetTimeAnalysisAsync(_profileId);

        // When no matches, the service returns empty TimeAnalysisDto
        Assert.NotNull(result);
        Assert.Empty(result.ByHour);
        Assert.Empty(result.ByDayOfWeek);
    }

    [Fact]
    public async Task GetTimeAnalysisAsync_CalculatesWinrateByHour()
    {
        var now = DateTime.Today.AddHours(14); // 2 PM
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Win = true, Date = now },
            new MatchEntry { ProfileId = _profileId, Win = true, Date = now.AddMinutes(30) },
            new MatchEntry { ProfileId = _profileId, Win = false, Date = now.AddMinutes(45) }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetTimeAnalysisAsync(_profileId);

        var hour14 = result.ByHour.First(h => h.Hour == 14);
        Assert.Equal(3, hour14.Games);
        Assert.Equal(2.0 / 3.0, hour14.Winrate, 2);
    }

    #endregion

    #region GetBestDuosAsync Tests

    [Fact]
    public async Task GetBestDuosAsync_NoMatches_ReturnsEmptyList()
    {
        var result = await _service.GetBestDuosAsync(_profileId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBestDuosAsync_FindsBestDuos()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Role = "ADC", Champion = "Jinx", LaneAlly = "Lulu", Win = true },
            new MatchEntry { ProfileId = _profileId, Role = "ADC", Champion = "Jinx", LaneAlly = "Lulu", Win = true },
            new MatchEntry { ProfileId = _profileId, Role = "ADC", Champion = "Vayne", LaneAlly = "Thresh", Win = false },
            new MatchEntry { ProfileId = _profileId, Role = "ADC", Champion = "Vayne", LaneAlly = "Thresh", Win = false }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetBestDuosAsync(_profileId);

        Assert.NotEmpty(result);
        Assert.Equal("Jinx", result[0].Champion);
        Assert.Equal("Lulu", result[0].Support);
        Assert.Equal(100, result[0].WinRate);
    }

    [Fact]
    public async Task GetBestDuosAsync_RequiresMinimumGames()
    {
        // Only 1 game - should not appear
        _db.Matches.Add(new MatchEntry 
        { 
            ProfileId = _profileId, 
            Role = "ADC", 
            Champion = "Jinx", 
            LaneAlly = "Lulu", 
            Win = true 
        });
        await _db.SaveChangesAsync();

        var result = await _service.GetBestDuosAsync(_profileId);

        Assert.Empty(result); // Needs at least 2 games
    }

    #endregion

    #region GetEnchanterUsageAsync Tests

    [Fact]
    public async Task GetEnchanterUsageAsync_NoMatches_ReturnsZeroPercentage()
    {
        var result = await _service.GetEnchanterUsageAsync(_profileId);

        Assert.Equal(0, result.MyPercentage);
        Assert.Equal(0, result.EnemyPercentage);
    }

    [Fact]
    public async Task GetEnchanterUsageAsync_CalculatesEnchanterPercentage()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Role = "ADC", LaneAlly = "Lulu", LaneEnemyAlly = "Thresh" },
            new MatchEntry { ProfileId = _profileId, Role = "ADC", LaneAlly = "Janna", LaneEnemyAlly = "Nautilus" },
            new MatchEntry { ProfileId = _profileId, Role = "ADC", LaneAlly = "Thresh", LaneEnemyAlly = "Soraka" }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetEnchanterUsageAsync(_profileId);

        // Lulu, Janna are enchanters; Thresh is not
        Assert.True(result.MyPercentage > 50); // 2/3 enchanters
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task GetChampionStatsAsync_FiltersbyDateRange()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true, Date = DateTime.UtcNow },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Win = true, Date = DateTime.UtcNow.AddMonths(-2) }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetChampionStatsAsync(
            _profileId, 
            startDate: DateTime.UtcNow.AddDays(-7));

        Assert.Single(result);
        Assert.Equal("Jinx", result[0].ChampionName);
    }

    [Fact]
    public async Task GetChampionStatsAsync_FiltersByGameMode()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true, GameMode = "Ranked Solo/Duo" },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Win = true, GameMode = "Normal Draft" }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetChampionStatsAsync(_profileId, gameMode: "Ranked Solo/Duo");

        Assert.Single(result);
        Assert.Equal("Jinx", result[0].ChampionName);
    }

    #endregion

    public void Dispose()
    {
        _db.Dispose();
    }
}
