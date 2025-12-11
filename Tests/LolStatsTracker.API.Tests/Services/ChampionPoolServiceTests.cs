using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.ChampionPoolService;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class ChampionPoolServiceTests : IDisposable
{
    private readonly MatchDbContext _db;
    private readonly ChampionPoolService _service;
    private readonly Guid _profileId = Guid.NewGuid();

    public ChampionPoolServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new MatchDbContext(options);
        _service = new ChampionPoolService(_db);

        // Seed a user profile
        _db.UserProfiles.Add(new UserProfile { Id = _profileId, SummonerName = "TestUser" });
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetPoolAsync_EmptyPool_ReturnsEmptyList()
    {
        var result = await _service.GetPoolAsync(_profileId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_AddsChampionToPool()
    {
        var dto = new ChampionPoolCreateDto("Jinx", "Primary", 1);
        
        var result = await _service.CreateAsync(_profileId, dto);
        
        Assert.NotNull(result);
        Assert.Equal("Jinx", result.Champion);
        Assert.Equal("Primary", result.Tier);
        Assert.Equal(1, result.Priority);
    }

    [Fact]
    public async Task GetPoolAsync_WithChampions_ReturnsWithStats()
    {
        // Add champion to pool
        await _service.CreateAsync(_profileId, new ChampionPoolCreateDto("Jinx", "Primary", 1));
        
        // Add some matches for that champion
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = true, Kills = 10, Deaths = 2, Assists = 5 },
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Win = false, Kills = 5, Deaths = 5, Assists = 10 }
        );
        await _db.SaveChangesAsync();
        
        var result = await _service.GetPoolAsync(_profileId);
        
        Assert.Single(result);
        Assert.Equal("Jinx", result[0].Champion);
        Assert.Equal(2, result[0].GamesPlayed);
        Assert.Equal(0.5, result[0].Winrate);
    }

    [Fact]
    public async Task UpdateAsync_ChangesTier()
    {
        var created = await _service.CreateAsync(_profileId, new ChampionPoolCreateDto("Vayne", "Secondary", 1));
        
        var updated = await _service.UpdateAsync(created.Id, new ChampionPoolUpdateDto("Primary", 1));
        
        Assert.NotNull(updated);
        Assert.Equal("Primary", updated.Tier);
    }

    [Fact]
    public async Task DeleteAsync_RemovesChampion()
    {
        var created = await _service.CreateAsync(_profileId, new ChampionPoolCreateDto("Ezreal", "Pocket", 1));
        
        var deleted = await _service.DeleteAsync(created.Id);
        var pool = await _service.GetPoolAsync(_profileId);
        
        Assert.True(deleted);
        Assert.Empty(pool);
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
