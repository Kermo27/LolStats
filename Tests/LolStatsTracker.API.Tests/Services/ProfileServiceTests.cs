using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class ProfileServiceTests : IDisposable
{
    private readonly MatchDbContext _db;
    private readonly ProfileService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public ProfileServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new MatchDbContext(options);
        _service = new ProfileService(_db);

        // Create a user
        _db.Users.Add(new User { Id = _userId, Username = "TestUser", PasswordHash = "hash" });
        _db.SaveChanges();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_NoProfiles_ReturnsEmptyList()
    {
        var result = await _service.GetAllAsync(_userId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProfilesForUser()
    {
        _db.UserProfiles.AddRange(
            new UserProfile { Id = Guid.NewGuid(), UserId = _userId, Name = "Profile1", Tag = "EUW" },
            new UserProfile { Id = Guid.NewGuid(), UserId = _userId, Name = "Profile2", Tag = "NA" },
            new UserProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "OtherUser", Tag = "KR" }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(_userId);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(_userId, p.UserId));
    }

    [Fact]
    public async Task GetAllAsync_OrdersDefaultProfileFirst()
    {
        _db.UserProfiles.AddRange(
            new UserProfile { Id = Guid.NewGuid(), UserId = _userId, Name = "Secondary", IsDefault = false },
            new UserProfile { Id = Guid.NewGuid(), UserId = _userId, Name = "Primary", IsDefault = true }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(_userId);

        Assert.Equal("Primary", result[0].Name);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingProfile_ReturnsProfile()
    {
        var profile = new UserProfile { Id = Guid.NewGuid(), UserId = _userId, Name = "Test", Tag = "EUW" };
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var result = await _service.GetByIdAsync(profile.Id, _userId);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentProfile_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), _userId);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ProfileFromDifferentUser_ReturnsNull()
    {
        var otherUserId = Guid.NewGuid();
        var profile = new UserProfile { Id = Guid.NewGuid(), UserId = otherUserId, Name = "Other" };
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var result = await _service.GetByIdAsync(profile.Id, _userId);

        Assert.Null(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_FirstProfile_MarksAsDefault()
    {
        var profile = new UserProfile { Name = "First Profile", Tag = "EUW" };

        var result = await _service.CreateAsync(profile, _userId);

        Assert.True(result.IsDefault);
        Assert.Equal(_userId, result.UserId);
    }

    [Fact]
    public async Task CreateAsync_SecondProfile_NotDefault()
    {
        _db.UserProfiles.Add(new UserProfile 
        { 
            Id = Guid.NewGuid(), 
            UserId = _userId, 
            Name = "First", 
            IsDefault = true 
        });
        await _db.SaveChangesAsync();

        var profile = new UserProfile { Name = "Second Profile", Tag = "NA" };

        var result = await _service.CreateAsync(profile, _userId);

        Assert.False(result.IsDefault);
    }

    [Fact]
    public async Task CreateAsync_AssignsUserId()
    {
        var profile = new UserProfile { Name = "Test", Tag = "EUW" };

        var result = await _service.CreateAsync(profile, _userId);

        Assert.Equal(_userId, result.UserId);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingProfile_UpdatesProperties()
    {
        var profile = new UserProfile 
        { 
            Id = Guid.NewGuid(), 
            UserId = _userId, 
            Name = "OldName", 
            Tag = "EUW" 
        };
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var updatedProfile = new UserProfile 
        { 
            Id = profile.Id, 
            Name = "NewName", 
            Tag = "NA",
            RiotPuuid = "new-puuid"
        };

        var result = await _service.UpdateAsync(updatedProfile, _userId);

        Assert.Equal("NewName", result.Name);
        Assert.Equal("NA", result.Tag);
        Assert.Equal("new-puuid", result.RiotPuuid);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentProfile_ThrowsException()
    {
        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "Test" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(profile, _userId));
    }

    [Fact]
    public async Task UpdateAsync_ProfileFromDifferentUser_ThrowsException()
    {
        var otherUserId = Guid.NewGuid();
        var profile = new UserProfile 
        { 
            Id = Guid.NewGuid(), 
            UserId = otherUserId, 
            Name = "Other" 
        };
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var updatedProfile = new UserProfile { Id = profile.Id, Name = "Hacked" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(updatedProfile, _userId));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingProfile_RemovesFromDatabase()
    {
        var profile = new UserProfile { Id = Guid.NewGuid(), UserId = _userId, Name = "ToDelete" };
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var result = await _service.DeleteAsync(profile.Id, _userId);

        Assert.True(result);
        Assert.Equal(0, await _db.UserProfiles.CountAsync(p => p.UserId == _userId));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentProfile_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), _userId);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ProfileFromDifferentUser_ReturnsFalse()
    {
        var otherUserId = Guid.NewGuid();
        var profile = new UserProfile { Id = Guid.NewGuid(), UserId = otherUserId, Name = "Other" };
        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var result = await _service.DeleteAsync(profile.Id, _userId);

        Assert.False(result);
        Assert.Equal(1, await _db.UserProfiles.CountAsync()); // Profile still exists
    }

    #endregion

    public void Dispose()
    {
        _db.Dispose();
    }
}
