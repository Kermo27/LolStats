using FluentAssertions;
using LolStatsTracker.Shared.Models;
using System.Text.Json;

namespace LolStatsTracker.Shared.Tests;

public class UserProfileTests
{
    [Fact]
    public void DefaultValues_ShouldBeSet()
    {
        var profile = new UserProfile();
        profile.Name.Should().Be("Main Account");
        profile.Tag.Should().Be("EUW");
        profile.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void JsonSerialize_And_Deserialize_Roundtrip()
    {
        var profile = new UserProfile { Id = Guid.NewGuid(), Name = "JsonTest", Tag = "KR", IsDefault = true };
        var json = JsonSerializer.Serialize(profile);
        var deserialized = JsonSerializer.Deserialize<UserProfile>(json);
        deserialized.Should().NotBeNull();
        deserialized.Id.Should().Be(profile.Id);
        deserialized.Name.Should().Be(profile.Name);
        deserialized.Tag.Should().Be(profile.Tag);
        deserialized.IsDefault.Should().Be(profile.IsDefault);
    }
}
