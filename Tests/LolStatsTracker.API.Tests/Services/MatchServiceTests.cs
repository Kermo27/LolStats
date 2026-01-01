using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.MatchService;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class MatchServiceTests : IDisposable
{
    private readonly MatchDbContext _db;
    private readonly MatchService _service;
    private readonly Guid _profileId = Guid.NewGuid();

    public MatchServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new MatchDbContext(options);
        _service = new MatchService(_db);

        _db.UserProfiles.Add(new UserProfile { Id = _profileId, Name = "TestUser" });
        _db.SaveChanges();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var result = await _service.GetAllAsync(_profileId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMatches_ReturnsMatchesForProfile()
    {
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Date = DateTime.UtcNow },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Date = DateTime.UtcNow.AddDays(-1) },
            new MatchEntry { ProfileId = Guid.NewGuid(), Champion = "Ezreal", Date = DateTime.UtcNow } // Different profile
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(_profileId);

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(_profileId, m.ProfileId));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMatchesOrderedByDateDescending()
    {
        var oldMatch = new MatchEntry { ProfileId = _profileId, Champion = "Jinx", Date = DateTime.UtcNow.AddDays(-5) };
        var newMatch = new MatchEntry { ProfileId = _profileId, Champion = "Vayne", Date = DateTime.UtcNow };
        
        _db.Matches.AddRange(oldMatch, newMatch);
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(_profileId);

        Assert.Equal("Vayne", result[0].Champion);
        Assert.Equal("Jinx", result[1].Champion);
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_ExistingMatch_ReturnsMatch()
    {
        var match = new MatchEntry { Id = Guid.NewGuid(), ProfileId = _profileId, Champion = "Jinx" };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        var result = await _service.GetAsync(match.Id, _profileId);

        Assert.NotNull(result);
        Assert.Equal("Jinx", result.Champion);
    }

    [Fact]
    public async Task GetAsync_NonExistentMatch_ReturnsNull()
    {
        var result = await _service.GetAsync(Guid.NewGuid(), _profileId);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_MatchFromDifferentProfile_ReturnsNull()
    {
        var match = new MatchEntry { Id = Guid.NewGuid(), ProfileId = Guid.NewGuid(), Champion = "Jinx" };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        var result = await _service.GetAsync(match.Id, _profileId);

        Assert.Null(result);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ValidMatch_AddsToDatabase()
    {
        var match = new MatchEntry 
        { 
            ProfileId = _profileId, 
            Champion = "Jinx", 
            Win = true,
            Kills = 10,
            Deaths = 2,
            Assists = 5
        };

        var result = await _service.AddAsync(match);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(1, await _db.Matches.CountAsync());
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingMatch_UpdatesProperties()
    {
        var match = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            Champion = "Jinx",
            Kills = 5,
            Deaths = 3,
            Assists = 10
        };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        var updatedMatch = new MatchEntry 
        { 
            Champion = "Vayne",
            Kills = 15,
            Deaths = 1,
            Assists = 5
        };

        var result = await _service.UpdateAsync(match.Id, updatedMatch, _profileId);

        Assert.NotNull(result);
        Assert.Equal("Vayne", result.Champion);
        Assert.Equal(15, result.Kills);
        Assert.Equal(1, result.Deaths);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentMatch_ReturnsNull()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), new MatchEntry(), _profileId);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_MatchFromDifferentProfile_ReturnsNull()
    {
        var match = new MatchEntry { Id = Guid.NewGuid(), ProfileId = Guid.NewGuid(), Champion = "Jinx" };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        var result = await _service.UpdateAsync(match.Id, new MatchEntry(), _profileId);

        Assert.Null(result);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingMatch_RemovesFromDatabase()
    {
        var match = new MatchEntry { Id = Guid.NewGuid(), ProfileId = _profileId, Champion = "Jinx" };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        var result = await _service.DeleteAsync(match.Id);

        Assert.True(result);
        Assert.Equal(0, await _db.Matches.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NonExistentMatch_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_RemovesAllMatchesForProfile()
    {
        var otherProfileId = Guid.NewGuid();
        _db.Matches.AddRange(
            new MatchEntry { ProfileId = _profileId, Champion = "Jinx" },
            new MatchEntry { ProfileId = _profileId, Champion = "Vayne" },
            new MatchEntry { ProfileId = otherProfileId, Champion = "Ezreal" }
        );
        await _db.SaveChangesAsync();

        await _service.ClearAsync(_profileId);

        Assert.Equal(1, await _db.Matches.CountAsync()); // Only the other profile's match remains
        Assert.Equal(otherProfileId, (await _db.Matches.FirstAsync()).ProfileId);
    }

    [Fact]
    public async Task ClearAsync_EmptyProfile_DoesNotThrow()
    {
        await _service.ClearAsync(_profileId);
        Assert.Equal(0, await _db.Matches.CountAsync());
    }

    #endregion

    public void Dispose()
    {
        _db.Dispose();
    }
}
