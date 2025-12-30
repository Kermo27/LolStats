using LolStatsTracker.Shared.Helpers;
using Xunit;

namespace LolStatsTracker.Shared.Tests;

public class PerformanceScoreHelperTests
{
    [Theory]
    // Formula: KDA * 4 (max 40) + CS/min * 3 (max 30) + win bonus (30)
    // Good game: (10+15)/2=12.5 KDA -> 40pts, 200/30=6.7 cs/min -> 20pts, +30 win = 90
    [InlineData(10, 2, 15, 200, 30, true, 90)]
    // Terrible game: (0+0)/10=0 KDA -> 0pts, 50/30=1.7 cs/min -> 5pts, +0 loss = 5  
    [InlineData(0, 10, 0, 50, 30, false, 5)]
    // Average with win: (5+5)/5=2 KDA -> 8pts, 150/30=5 cs/min -> 15pts, +30 win = 53
    [InlineData(5, 5, 5, 150, 30, true, 53)]
    // Average with loss: (5+5)/5=2 KDA -> 8pts, 150/30=5 cs/min -> 15pts, +0 loss = 23
    [InlineData(5, 5, 5, 150, 30, false, 23)]
    public void Calculate_ReturnsExpectedRange(
        int kills, int deaths, int assists, int cs, int gameLength, bool win, int approxExpected)
    {
        var result = PerformanceScoreHelper.Calculate(kills, deaths, assists, cs, gameLength, win);
        
        // Allow +/- 5 tolerance for rounding
        Assert.InRange(result, approxExpected - 5, approxExpected + 5);
    }

    [Fact]
    public void Calculate_ReturnsScoreBetween0And100()
    {
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
