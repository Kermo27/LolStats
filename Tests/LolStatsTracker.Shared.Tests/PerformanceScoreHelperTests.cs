using LolStatsTracker.Shared.Helpers;
using Xunit;

namespace LolStatsTracker.Shared.Tests;

public class PerformanceScoreHelperTests
{
    [Theory]
    [InlineData(10, 2, 15, 200, 30, true, 85)]   // Great game
    [InlineData(0, 10, 0, 50, 30, false, 5)]     // Terrible game
    [InlineData(5, 5, 5, 150, 30, true, 60)]     // Average game with win
    [InlineData(5, 5, 5, 150, 30, false, 45)]    // Average game with loss
    public void Calculate_ReturnsExpectedRange(
        int kills, int deaths, int assists, int cs, int gameLength, bool win, int approxExpected)
    {
        var result = PerformanceScoreHelper.Calculate(kills, deaths, assists, cs, gameLength, win);
        
        // Allow +/- 15 tolerance since exact formula may vary
        Assert.InRange(result, approxExpected - 15, approxExpected + 15);
    }

    [Fact]
    public void Calculate_ReturnsScoreBetween0And100()
    {
        // Test various edge cases
        var extremeGood = PerformanceScoreHelper.Calculate(30, 0, 20, 400, 25, true);
        var extremeBad = PerformanceScoreHelper.Calculate(0, 15, 0, 20, 30, false);
        
        Assert.InRange(extremeGood, 0, 100);
        Assert.InRange(extremeBad, 0, 100);
    }

    [Theory]
    [InlineData(90, "S+")]
    [InlineData(80, "S")]
    [InlineData(70, "A")]
    [InlineData(60, "B")]
    [InlineData(50, "C")]
    [InlineData(40, "D")]
    [InlineData(30, "F")]
    public void GetRating_ReturnsCorrectGrade(int score, string expectedRating)
    {
        var result = PerformanceScoreHelper.GetRating(score);
        Assert.Equal(expectedRating, result);
    }

    [Fact]
    public void Win_IncreasesScore()
    {
        var scoreWithWin = PerformanceScoreHelper.Calculate(5, 5, 5, 150, 30, true);
        var scoreWithLoss = PerformanceScoreHelper.Calculate(5, 5, 5, 150, 30, false);
        
        Assert.True(scoreWithWin > scoreWithLoss);
    }

    [Fact]
    public void HighKDA_IncreasesScore()
    {
        var highKda = PerformanceScoreHelper.Calculate(10, 2, 10, 150, 30, true);
        var lowKda = PerformanceScoreHelper.Calculate(2, 10, 2, 150, 30, true);
        
        Assert.True(highKda > lowKda);
    }
}
