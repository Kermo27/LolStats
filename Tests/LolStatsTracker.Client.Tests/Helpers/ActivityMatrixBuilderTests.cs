using LolStatsTracker.Helpers;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Client.Tests.Helpers;

public class ActivityMatrixBuilderTests
{
    [Fact]
    public void Build_ShouldGenerateMatrix_WithCorrectNumberOfWeeks()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var inputData = new List<ActivityDayDto>
        {
            new ActivityDayDto { Date = today, GamesPlayed = 5 },
        };

        var builder = new ActivityMatrixBuilder(inputData);
        
        // Act
        new matrix = builder.Build();
        
        // Assert
        matrix.Should().NotBeNull();
        matrix.Should().NotBeEmpty();

        foreach (var week in matrix)
        {
            week.Should().HaveCount(7);
        }

        var flatList = matrix.SelectMany(x => x).ToList();
        var todayEntry = flatList.FirstOrDefault(x => DateOnly.FromDateTime(x.Date) == today);

        todayEntry.Count.Should().Be(5);
    }
}