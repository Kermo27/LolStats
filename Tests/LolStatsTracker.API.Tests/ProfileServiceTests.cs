using FluentAssertions;
using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Tests;

public class ProfileServiceTests
{
    private MatchDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<MatchDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new MatchDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_FirstProfile_ShouldBeDefault()
    {
        using var context = CreateInMemoryDb("Create_First_Default");
        var service = new ProfileService(context);

        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "Test", Tag = "EUW" };
        var created = await service.CreateAsync(profile);

        created.IsDefault.Should().BeTrue();

        var all = await service.GetAllAsync();
        all.Should().ContainSingle();
        all[0].IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_SecondProfile_ShouldNotBeDefault()
    {
        using var context = CreateInMemoryDb("Create_Second_NotDefault");
        var service = new ProfileService(context);

        var first = new UserProfile { Id = Guid.NewGuid(), Name = "First", Tag = "EUW" };
        await service.CreateAsync(first);

        var second = new UserProfile { Id = Guid.NewGuid(), Name = "Second", Tag = "NA" };
        var createdSecond = await service.CreateAsync(second);

        createdSecond.IsDefault.Should().BeFalse();

        var all = await service.GetAllAsync();
        all.Should().HaveCount(2);
        all.Where(p => p.IsDefault).Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteAsync_RemovesProfile_ReturnsTrue()
    {
        using var context = CreateInMemoryDb("Delete_Profile");
        var service = new ProfileService(context);

        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "ToDelete", Tag = "EUW" };
        var created = await service.CreateAsync(profile);

        var result = await service.DeleteAsync(created.Id);
        result.Should().BeTrue();

        var fetched = await service.GetByIdAsync(created.Id);
        fetched.Should().BeNull();
    }
}

