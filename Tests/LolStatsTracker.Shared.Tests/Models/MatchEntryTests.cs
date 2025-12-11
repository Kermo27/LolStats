using FluentAssertions;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Shared.Tests.Models;

public class MatchEntryTests
{
    [Fact]
    public void KdaDisplay_ShouldReturnCorrectFormat()
    {
        // Arrange
        var match = new MatchEntry
        {
            Kills = 10,
            Deaths = 2,
            Assists = 3
        };
        
        // Act
        var kda = match.KdaDisplay;
        
        // Assert
        kda.Should().Be("10/2/3");
    }

    [Fact]
    public void Win_ShouldBeFalse_WhenInitialized()
    {
        // Arrange & Act
        var match = new MatchEntry();
        
        // Assert
        match.Win.Should().BeFalse();
    }
}