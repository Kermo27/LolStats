using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.MilestoneService;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class MilestoneServiceTests : IDisposable
{
    private readonly MatchDbContext _db;
    private readonly MilestoneService _service;
    private readonly Guid _profileId = Guid.NewGuid();

    public MilestoneServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new MatchDbContext(options);
        _service = new MilestoneService(_db);

        _db.UserProfiles.Add(new UserProfile { Id = _profileId, Name = "TestUser" });
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetMilestonesAsync_Empty_ReturnsEmptyList()
    {
        var result = await _service.GetMilestonesAsync(_profileId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_AddsMilestone()
    {
        var dto = new RankMilestoneCreateDto("Gold", 4, DateTime.UtcNow);
        
        var result = await _service.CreateAsync(_profileId, dto);
        
        Assert.NotNull(result);
        Assert.Equal("Gold", result.Tier);
        Assert.Equal(4, result.Division);
        Assert.Equal("Manual", result.Type);
    }

    [Fact]
    public async Task CheckAndRecordMilestoneAsync_Promotion_CreatesMilestone()
    {
        var previousMatch = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            CurrentTier = "Silver", 
            CurrentDivision = 1,
            Date = DateTime.UtcNow.AddDays(-1)
        };
        
        var newMatch = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            CurrentTier = "Gold", 
            CurrentDivision = 4,
            Date = DateTime.UtcNow
        };
        
        await _service.CheckAndRecordMilestoneAsync(_profileId, newMatch, previousMatch);
        
        var milestones = await _service.GetMilestonesAsync(_profileId);
        Assert.Single(milestones);
        Assert.Equal("Promotion", milestones[0].Type);
        Assert.Equal("Gold", milestones[0].Tier);
    }

    [Fact]
    public async Task CheckAndRecordMilestoneAsync_Demotion_CreatesMilestone()
    {
        var previousMatch = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            CurrentTier = "Gold", 
            CurrentDivision = 4,
            Date = DateTime.UtcNow.AddDays(-1)
        };
        
        var newMatch = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            CurrentTier = "Silver", 
            CurrentDivision = 1,
            Date = DateTime.UtcNow
        };
        
        await _service.CheckAndRecordMilestoneAsync(_profileId, newMatch, previousMatch);
        
        var milestones = await _service.GetMilestonesAsync(_profileId);
        Assert.Single(milestones);
        Assert.Equal("Demotion", milestones[0].Type);
    }

    [Fact]
    public async Task CheckAndRecordMilestoneAsync_NoChange_DoesNotCreateMilestone()
    {
        var previousMatch = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            CurrentTier = "Gold", 
            CurrentDivision = 2,
            Date = DateTime.UtcNow.AddDays(-1)
        };
        
        var newMatch = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            CurrentTier = "Gold", 
            CurrentDivision = 2,
            Date = DateTime.UtcNow
        };
        
        await _service.CheckAndRecordMilestoneAsync(_profileId, newMatch, previousMatch);
        
        var milestones = await _service.GetMilestonesAsync(_profileId);
        Assert.Empty(milestones);
    }

    [Fact]
    public async Task CheckAndRecordMilestoneAsync_NoPreviousMatch_DoesNothing()
    {
        var newMatch = new MatchEntry 
        { 
            Id = Guid.NewGuid(), 
            ProfileId = _profileId, 
            CurrentTier = "Gold", 
            CurrentDivision = 4
        };
        
        await _service.CheckAndRecordMilestoneAsync(_profileId, newMatch, null);
        
        var milestones = await _service.GetMilestonesAsync(_profileId);
        Assert.Empty(milestones);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
