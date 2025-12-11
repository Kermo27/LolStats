using LolStatsTracker.Shared.Helpers;
using LolStatsTracker.Shared.Models;
using Xunit;

namespace LolStatsTracker.Shared.Tests;

public class ChampionStatsHelperTests
{
    [Theory]
    [InlineData(10, 5, 10, 4.0)]    // (10+10)/5 = 4.0
    [InlineData(5, 0, 5, 10.0)]     // 0 deaths = K+A = 10
    [InlineData(0, 0, 0, 0.0)]      // 0/0/0 = 0
    [InlineData(3, 3, 3, 2.0)]      // (3+3)/3 = 2.0
    [InlineData(10, 1, 5, 15.0)]    // (10+5)/1 = 15
    public void CalculateKda_ReturnsCorrectValue(int kills, int deaths, int assists, double expected)
    {
        var result = ChampionStatsHelper.CalculateKda(kills, deaths, assists);
        Assert.Equal(expected, result, 2);
    }

    [Theory]
    [InlineData(5, 10, 0.5)]
    [InlineData(10, 10, 1.0)]
    [InlineData(0, 10, 0.0)]
    [InlineData(0, 0, 0.0)]     // Edge case: no games
    [InlineData(7, 10, 0.7)]
    public void CalculateWinrate_ReturnsCorrectValue(int wins, int total, double expected)
    {
        var result = ChampionStatsHelper.CalculateWinrate(wins, total);
        Assert.Equal(expected, result, 2);
    }

    [Fact]
    public void CalculateStats_EmptyList_ReturnsZeros()
    {
        var matches = new List<MatchEntry>();
        
        var result = ChampionStatsHelper.CalculateStats(matches);
        
        Assert.Equal(0, result.Games);
        Assert.Equal(0, result.Wins);
        Assert.Equal(0.0, result.Winrate);
        Assert.Equal(0.0, result.AvgKda);
    }

    [Fact]
    public void CalculateStats_WithMatches_ReturnsCorrectStats()
    {
        var matches = new List<MatchEntry>
        {
            CreateMatch(5, 2, 10, true, 200, 30),   // KDA = 7.5, CSpm = 6.67
            CreateMatch(3, 3, 6, false, 180, 30),  // KDA = 3.0, CSpm = 6.0
            CreateMatch(10, 0, 5, true, 250, 25),  // KDA = 15, CSpm = 10.0
        };
        
        var result = ChampionStatsHelper.CalculateStats(matches);
        
        Assert.Equal(3, result.Games);
        Assert.Equal(2, result.Wins);
        Assert.Equal(2.0/3.0, result.Winrate, 2);
        Assert.True(result.AvgKda > 0);
    }

    [Fact]
    public void GroupByChampion_GroupsCorrectly()
    {
        var matches = new List<MatchEntry>
        {
            CreateMatch(5, 2, 10, true, 200, 30, "Jinx"),
            CreateMatch(3, 3, 6, false, 180, 30, "Jinx"),
            CreateMatch(10, 1, 5, true, 250, 25, "Vayne"),
        };
        
        var result = ChampionStatsHelper.GroupByChampion(matches);
        
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Jinx"));
        Assert.True(result.ContainsKey("Vayne"));
        Assert.Equal(2, result["Jinx"].Games);
        Assert.Equal(1, result["Vayne"].Games);
    }

    private static MatchEntry CreateMatch(
        int kills, int deaths, int assists, bool win, 
        int cs = 0, int gameLength = 30, string champion = "TestChamp")
    {
        return new MatchEntry
        {
            Id = Guid.NewGuid(),
            Champion = champion,
            Kills = kills,
            Deaths = deaths,
            Assists = assists,
            Win = win,
            Cs = cs,
            GameLengthMinutes = gameLength,
            Date = DateTime.UtcNow
        };
    }
}
