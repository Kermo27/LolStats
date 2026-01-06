using LolStatsTracker.API.Services.StatsService;
using LolStatsTracker.Shared.Models;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class StatsCalculatorTests
{
    private readonly Guid _profileId = Guid.NewGuid();

    private MatchEntry CreateMatch(string champion, bool win, string role = "bot", int kills = 5, int deaths = 3, int assists = 10, DateTime? date = null)
    {
        return new MatchEntry
        {
            Id = Guid.NewGuid(),
            ProfileId = _profileId,
            Champion = champion,
            Win = win,
            Role = role,
            LaneAlly = "Thresh",
            LaneEnemy = "Jinx",
            LaneEnemyAlly = "Blitzcrank",
            Kills = kills,
            Deaths = deaths,
            Assists = assists,
            Cs = 200,
            GameLengthMinutes = 30,
            Date = date ?? DateTime.UtcNow,
            CurrentTier = "Gold",
            CurrentDivision = 2,
            CurrentLp = 50,
            GameMode = "Ranked Solo",
            QueueId = 420
        };
    }

    [Fact]
    public void ComputeOverview_EmptyList_ReturnsZeroWinrate()
    {
        // Arrange
        var matches = new List<MatchEntry>();

        // Act
        var result = StatsCalculator.ComputeOverview(matches);

        // Assert
        Assert.Equal(0, result.Winrate);
        Assert.Empty(result.MostPlayedChampion);
    }

    [Fact]
    public void ComputeOverview_WithMatches_CalculatesCorrectWinrate()
    {
        // Arrange
        var matches = new List<MatchEntry>
        {
            CreateMatch("Jinx", win: true),
            CreateMatch("Jinx", win: true),
            CreateMatch("Caitlyn", win: false),
            CreateMatch("Jinx", win: true)
        };

        // Act
        var result = StatsCalculator.ComputeOverview(matches);

        // Assert
        Assert.Equal(0.75, result.Winrate, precision: 2);
        Assert.Equal("Jinx", result.MostPlayedChampion);
        Assert.Equal(3, result.MostPlayedChampionGames);
    }

    [Fact]
    public void ComputeChampionStats_GroupsByChampion()
    {
        // Arrange
        var matches = new List<MatchEntry>
        {
            CreateMatch("Jinx", win: true, kills: 10, deaths: 2, assists: 5),
            CreateMatch("Jinx", win: true, kills: 8, deaths: 3, assists: 7),
            CreateMatch("Caitlyn", win: false, kills: 3, deaths: 5, assists: 2)
        };

        // Act
        var result = StatsCalculator.ComputeChampionStats(matches);

        // Assert
        Assert.Equal(2, result.Count);
        
        var jinxStats = result.First(c => c.ChampionName == "Jinx");
        Assert.Equal(2, jinxStats.Games);
        Assert.Equal(1.0, jinxStats.Winrate, precision: 2);
        
        var caitStats = result.First(c => c.ChampionName == "Caitlyn");
        Assert.Equal(1, caitStats.Games);
        Assert.Equal(0.0, caitStats.Winrate, precision: 2);
    }

    [Fact]
    public void ComputeStreak_WithWinStreak_ReturnsCorrectStreak()
    {
        // Arrange - 3 wins in a row at the end
        var matches = new List<MatchEntry>
        {
            CreateMatch("Jinx", win: false, date: DateTime.UtcNow.AddHours(-5)),
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddHours(-4)),
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddHours(-3)),
            CreateMatch("Caitlyn", win: true, date: DateTime.UtcNow.AddHours(-2))
        };

        // Act
        var result = StatsCalculator.ComputeStreak(matches);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result.IsWinStreak);
    }

    [Fact]
    public void ComputeStreak_WithLossStreak_ReturnsNegativeStreak()
    {
        // Arrange - 2 losses in a row at the end
        var matches = new List<MatchEntry>
        {
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddHours(-3)),
            CreateMatch("Jinx", win: false, date: DateTime.UtcNow.AddHours(-2)),
            CreateMatch("Caitlyn", win: false, date: DateTime.UtcNow.AddHours(-1))
        };

        // Act
        var result = StatsCalculator.ComputeStreak(matches);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.False(result.IsWinStreak);
    }

    [Fact]
    public void ComputeTiltStatus_RecentLosses_ReturnsTilted()
    {
        // Arrange - 5 recent losses
        var matches = new List<MatchEntry>
        {
            CreateMatch("Jinx", win: false, date: DateTime.UtcNow.AddMinutes(-10)),
            CreateMatch("Jinx", win: false, date: DateTime.UtcNow.AddMinutes(-40)),
            CreateMatch("Jinx", win: false, date: DateTime.UtcNow.AddMinutes(-70)),
            CreateMatch("Jinx", win: false, date: DateTime.UtcNow.AddMinutes(-100)),
            CreateMatch("Jinx", win: false, date: DateTime.UtcNow.AddMinutes(-130))
        };

        // Act
        var result = StatsCalculator.ComputeTiltStatus(matches);

        // Assert
        Assert.True(result.IsTilted);
        Assert.Equal(0, result.RecentWinrate, precision: 2);
    }

    [Fact]
    public void ComputeTiltStatus_RecentWins_ReturnsNotTilted()
    {
        // Arrange - 5 recent wins
        var matches = new List<MatchEntry>
        {
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddMinutes(-10)),
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddMinutes(-40)),
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddMinutes(-70)),
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddMinutes(-100)),
            CreateMatch("Jinx", win: true, date: DateTime.UtcNow.AddMinutes(-130))
        };

        // Act
        var result = StatsCalculator.ComputeTiltStatus(matches);

        // Assert
        Assert.False(result.IsTilted);
        Assert.Equal(1.0, result.RecentWinrate, precision: 2);
    }

    [Fact]
    public void ComputeActivity_GroupsByDate()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var matches = new List<MatchEntry>
        {
            CreateMatch("Jinx", win: true, date: today.AddHours(10)),
            CreateMatch("Jinx", win: true, date: today.AddHours(12)),
            CreateMatch("Caitlyn", win: false, date: today.AddDays(-1).AddHours(15))
        };

        // Act
        var result = StatsCalculator.ComputeActivity(matches, 6);

        // Assert
        Assert.Equal(2, result.Count);
        var todayActivity = result.FirstOrDefault(a => a.Date == DateOnly.FromDateTime(today));
        Assert.NotNull(todayActivity);
        Assert.Equal(2, todayActivity.GamesPlayed);
    }
}
