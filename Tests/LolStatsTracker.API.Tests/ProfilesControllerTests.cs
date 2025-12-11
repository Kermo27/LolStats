using FluentAssertions;
using LolStatsTracker.API.Controllers;
using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LolStatsTracker.API.Tests;

public class ProfilesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithProfiles()
    {
        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UserProfile>
        {
            new UserProfile { Id = Guid.NewGuid(), Name = "A" },
            new UserProfile { Id = Guid.NewGuid(), Name = "B" }
        });

        var controller = new ProfilesController(mock.Object);
        var result = await controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        ok!.Value.Should().BeOfType<List<UserProfile>>();
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenFound()
    {
        var id = Guid.NewGuid();
        var profile = new UserProfile { Id = id, Name = "Found" };
        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(profile);

        var controller = new ProfilesController(mock.Object);
        var result = await controller.Get(id);

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        ok!.Value.Should().Be(profile);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenMissing()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((UserProfile?)null);

        var controller = new ProfilesController(mock.Object);
        var result = await controller.Get(id);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var mock = new Mock<IProfileService>();
        var controller = new ProfilesController(mock.Object);

        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "  ", Tag = "EUW" };
        var result = await controller.Create(profile);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "New", Tag = "EUW" };
        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.CreateAsync(It.IsAny<UserProfile>())).ReturnsAsync((UserProfile p) => p);

        var controller = new ProfilesController(mock.Object);
        var result = await controller.Create(profile);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var created = result.Result as CreatedAtActionResult;
        created!.Value.Should().Be(profile);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        var mock = new Mock<IProfileService>();
        var controller = new ProfilesController(mock.Object);

        var id = Guid.NewGuid();
        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "X" };

        var result = await controller.Update(id, profile);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        var profile = new UserProfile { Id = id, Name = "X" };
        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((UserProfile?)null);

        var controller = new ProfilesController(mock.Object);
        var result = await controller.Update(id, profile);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUpdated()
    {
        var id = Guid.NewGuid();
        var existing = new UserProfile { Id = id, Name = "Old", Tag = "EUW" };
        var updated = new UserProfile { Id = id, Name = "New", Tag = "NA" };

        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existing);
        mock.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>())).ReturnsAsync((UserProfile p) => p);

        var controller = new ProfilesController(mock.Object);
        var result = await controller.Update(id, updated);

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(existing);
        existing.Name.Should().Be(updated.Name);
        existing.Tag.Should().Be(updated.Tag);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        var controller = new ProfilesController(mock.Object);
        var result = await controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<IProfileService>();
        mock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        var controller = new ProfilesController(mock.Object);
        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundResult>();
    }
}

