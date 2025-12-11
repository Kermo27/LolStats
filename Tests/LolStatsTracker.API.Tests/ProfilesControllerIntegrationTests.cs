using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Linq;
using LolStatsTracker.API.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LolStatsTracker.API.Tests;

public class ProfilesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProfilesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Set the environment to Testing so Program won't register the real database provider
            builder.UseSetting("environment", "Testing");
            builder.ConfigureServices(services =>
            {
                // Replace MatchDbContext with an in-memory provider for tests
                // Remove existing registrations for MatchDbContext and its options so we don't have multiple providers registered
                services.RemoveAll(typeof(DbContextOptions<MatchDbContext>));
                services.RemoveAll(typeof(MatchDbContext));

                services.AddDbContext<MatchDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestsDb"));
            });
        });
    }

    [Fact]
    public async Task CreateGetDelete_Profile_Lifecycle()
    {
        using var client = _factory.CreateClient();

        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "ITTest", Tag = "EUW" };

        // Create
        var createResp = await client.PostAsJsonAsync("/api/profiles", profile);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<UserProfile>();
        created.Should().NotBeNull();
        created!.Name.Should().Be(profile.Name);

        // Get
        var getResp = await client.GetAsync($"/api/profiles/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResp.Content.ReadFromJsonAsync<UserProfile>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);

        // Delete
        var delResp = await client.DeleteAsync($"/api/profiles/{created.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get again -> NotFound
        var getAgain = await client.GetAsync($"/api/profiles/{created.Id}");
        getAgain.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
