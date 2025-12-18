using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.MetaService;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class MetaServiceTests : IDisposable
{
    private readonly MatchDbContext _db;
    private readonly MetaService _service;
    private readonly Guid _profileId = Guid.NewGuid();

    public MetaServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new MatchDbContext(options);
        _service = new MetaService(_db);

        _db.UserProfiles.Add(new UserProfile { Id = _profileId, Name = "TestUser" });
        _db.SaveChanges();
    }

    [Fact]
    public void GetTiers_ReturnsAllTiers()
    {
        var result = _service.GetTiers().ToList();
        
        Assert.NotEmpty(result);
        Assert.True(result.Count > 10); // ADC tier list has many champions
    }

    [Fact]
    public async Task GetComparisonAsync_NoMatches_ReturnsZeroPlayed()
    {
        var result = await _service.GetComparisonAsync(_profileId);
        
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalMetaPlayed);
        Assert.Equal(0, result.TotalOffMeta);
        Assert.All(result.MetaChampions, c => Assert.False(c.IsPlayed));
    }

    [Fact]
    public async Task GetComparisonAsync_WithMetaChampion_MarksAsPlayed()
    {
        // Add matches with meta champion (Jinx is S-tier)
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true }
        );
        await _db.SaveChangesAsync();
        
        var result = await _service.GetComparisonAsync(_profileId);
        
        var jinx = result.MetaChampions.First(c => c.Champion == "Jinx");
        Assert.True(jinx.IsPlayed);
        Assert.Equal(2, jinx.GamesPlayed);
        Assert.Equal(1.0, jinx.Winrate);
    }

    [Fact]
    public async Task GetComparisonAsync_WithSTierChampion_IncrementsMetaPlayed()
    {
        _db.Matches.Add(new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true });
        await _db.SaveChangesAsync();
        
        var result = await _service.GetComparisonAsync(_profileId);
        
        Assert.Equal(1, result.TotalMetaPlayed); // S/A tier count
    }

    [Fact]
    public async Task GetComparisonAsync_WithOffMetaPick_IncrementsOffMeta()
    {
        _db.Matches.Add(new MatchEntry { ProfileId = _profileId, Champion = "Teemo", Win = true });
        await _db.SaveChangesAsync();
        
        var result = await _service.GetComparisonAsync(_profileId);
        
        Assert.Equal(1, result.TotalOffMeta);
    }

    [Fact]
    public async Task GetComparisonAsync_GeneratesRecommendation()
    {
        var result = await _service.GetComparisonAsync(_profileId);
        
        Assert.NotNull(result.Recommendation);
        Assert.NotEmpty(result.Recommendation);
    }

    [Fact]
    public async Task GetComparisonAsync_ManyMetaPicks_GivesPositiveRecommendation()
    {
        // Add 3 S/A tier champions
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Kai'Sa", Win = true },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Win = true }
        );
        await _db.SaveChangesAsync();
        
        var result = await _service.GetComparisonAsync(_profileId);
        
        Assert.Contains("Great", result.Recommendation);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
