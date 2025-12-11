using LolStatsTracker.Shared.Constants;
using Xunit;

namespace LolStatsTracker.Shared.Tests;

public class MetaTierListTests
{
    [Fact]
    public void AdcTiers_ContainsAllTiers()
    {
        var tiers = MetaTierList.AdcTiers.Select(t => t.Tier).Distinct().ToList();
        
        Assert.Contains("S", tiers);
        Assert.Contains("A", tiers);
        Assert.Contains("B", tiers);
        Assert.Contains("C", tiers);
        Assert.Contains("D", tiers);
    }

    [Fact]
    public void AdcTiers_ContainsExpectedChampions()
    {
        var champions = MetaTierList.AdcTiers.Select(t => t.Champion).ToList();
        
        Assert.Contains("Jinx", champions);
        Assert.Contains("Kai'Sa", champions);
        Assert.Contains("Vayne", champions);
        Assert.Contains("Ezreal", champions);
    }

    [Theory]
    [InlineData("Jinx", true)]
    [InlineData("Kai'Sa", true)]
    [InlineData("jinx", true)]  // Case insensitive
    [InlineData("NonExistentChamp", false)]
    [InlineData("", false)]
    public void IsMetaChampion_ReturnsCorrectResult(string champion, bool expected)
    {
        var result = MetaTierList.IsMetaChampion(champion);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Jinx", "S")]
    [InlineData("Vayne", "A")]
    [InlineData("Samira", "B")]
    [InlineData("Sivir", "C")]
    [InlineData("Zeri", "D")]
    public void GetChampionTier_ReturnsCorrectTier(string champion, string expectedTier)
    {
        var result = MetaTierList.GetChampionTier(champion);
        Assert.Equal(expectedTier, result);
    }

    [Fact]
    public void GetChampionTier_NonExistent_ReturnsNull()
    {
        var result = MetaTierList.GetChampionTier("NotARealChampion");
        Assert.Null(result);
    }

    [Fact]
    public void GetChampionsInTier_S_ReturnsSTierChampions()
    {
        var sTierChamps = MetaTierList.GetChampionsInTier("S").ToList();
        
        Assert.Contains("Jinx", sTierChamps);
        Assert.Contains("Kai'Sa", sTierChamps);
        Assert.Contains("Jhin", sTierChamps);
        Assert.Contains("Caitlyn", sTierChamps);
        Assert.Equal(4, sTierChamps.Count);
    }

    [Fact]
    public void TierPriority_OrderedCorrectly()
    {
        Assert.True(MetaTierList.TierPriority["S"] < MetaTierList.TierPriority["A"]);
        Assert.True(MetaTierList.TierPriority["A"] < MetaTierList.TierPriority["B"]);
        Assert.True(MetaTierList.TierPriority["D"] == 5);
    }
}
