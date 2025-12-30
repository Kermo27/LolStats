using LolStatsTracker.Helpers;
using LolStatsTracker.Shared.DTOs;
using Xunit;

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
            new ActivityDayDto(today, 5),
        };

        var builder = new ActivityMatrixBuilder(inputData);
        
        // Act
        var matrix = builder.Build();
        
        // Assert
        Assert.NotNull(matrix);
        Assert.NotEmpty(matrix);

        foreach (var week in matrix)
        {
            Assert.Equal(7, week.Count);
        }

        var flatList = matrix.SelectMany(x => x).ToList();
        var todayEntry = flatList.FirstOrDefault(x => DateOnly.FromDateTime(x.Date) == today);

        Assert.Equal(5, todayEntry.Count);
    }

    [Fact]
    public void Build_EmptyInput_ReturnsMatrixWithZeroCounts()
    {
        // Arrange
        var inputData = new List<ActivityDayDto>();
        var builder = new ActivityMatrixBuilder(inputData);
        
        // Act
        var matrix = builder.Build();
        
        // Assert
        Assert.NotNull(matrix);
        Assert.NotEmpty(matrix);
        
        var flatList = matrix.SelectMany(x => x).ToList();
        Assert.All(flatList, cell => Assert.Equal(0, cell.Count));
    }

    [Fact]
    public void Build_MultipleDays_SumsGamesCorrectly()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);
        var inputData = new List<ActivityDayDto>
        {
            new ActivityDayDto(today, 3),
            new ActivityDayDto(yesterday, 2),
        };

        var builder = new ActivityMatrixBuilder(inputData);
        
        // Act
        var matrix = builder.Build();
        
        // Assert
        var flatList = matrix.SelectMany(x => x).ToList();
        
        var todayEntry = flatList.FirstOrDefault(x => DateOnly.FromDateTime(x.Date) == today);
        var yesterdayEntry = flatList.FirstOrDefault(x => DateOnly.FromDateTime(x.Date) == yesterday);

        Assert.Equal(3, todayEntry.Count);
        Assert.Equal(2, yesterdayEntry.Count);
    }
}