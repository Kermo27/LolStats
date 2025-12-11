using LolStatsTracker.Shared.Constants;
using Xunit;

namespace LolStatsTracker.Shared.Tests;

public class RankConstantsTests
{
    [Theory]
    [InlineData("Iron", 4, 10)]      // Iron IV = 1*10 + (4-4) = 10
    [InlineData("Iron", 1, 13)]      // Iron I = 1*10 + (4-1) = 13
    [InlineData("Gold", 4, 40)]      // Gold IV = 4*10 + (4-4) = 40
    [InlineData("Gold", 1, 43)]      // Gold I = 4*10 + (4-1) = 43
    [InlineData("Diamond", 1, 73)]   // Diamond I = 7*10 + (4-1) = 73
    [InlineData("Master", 0, 80)]    // Master = 8*10 = 80 (no division)
    [InlineData("Grandmaster", 0, 90)]
    [InlineData("Challenger", 0, 100)]
    public void GetRankValue_ReturnsCorrectValue(string tier, int division, int expected)
    {
        var result = RankConstants.GetRankValue(tier, division);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("InvalidTier", 1, 0)]
    [InlineData("", 1, 0)]
    public void GetRankValue_InvalidTier_ReturnsZero(string tier, int division, int expected)
    {
        var result = RankConstants.GetRankValue(tier, division);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, "I")]
    [InlineData(2, "II")]
    [InlineData(3, "III")]
    [InlineData(4, "IV")]
    [InlineData(0, "")]
    [InlineData(5, "")]
    public void GetDivisionRoman_ReturnsCorrectNumeral(int division, string expected)
    {
        var result = RankConstants.GetDivisionRoman(division);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Gold", 1, "Silver", 1, true)]   // Gold I > Silver I
    [InlineData("Gold", 4, "Gold", 1, false)]    // Gold IV < Gold I
    [InlineData("Master", 0, "Diamond", 1, true)] // Master > Diamond I
    [InlineData("Iron", 4, "Iron", 4, false)]    // Equal (not greater)
    public void CompareRanks_ReturnsCorrectComparison(
        string tier1, int div1, string tier2, int div2, bool firstIsHigher)
    {
        var result = RankConstants.CompareRanks(tier1, div1, tier2, div2);
        
        if (firstIsHigher)
            Assert.True(result > 0);
        else
            Assert.True(result <= 0);
    }

    [Fact]
    public void TierOrder_ContainsAllTiers()
    {
        Assert.Equal(10, RankConstants.TierOrder.Count);
        Assert.Contains("Iron", RankConstants.TierOrder.Keys);
        Assert.Contains("Challenger", RankConstants.TierOrder.Keys);
    }

    [Fact]
    public void ApexTiers_ContainsMasterAndAbove()
    {
        Assert.Contains("Master", RankConstants.ApexTiers);
        Assert.Contains("Grandmaster", RankConstants.ApexTiers);
        Assert.Contains("Challenger", RankConstants.ApexTiers);
        Assert.DoesNotContain("Diamond", RankConstants.ApexTiers);
    }
}
