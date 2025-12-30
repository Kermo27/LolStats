using LolStatsTracker.TrayApp.Helpers;
using Xunit;

namespace LolStatsTracker.TrayApp.Tests.Helpers;

public class DataMapperTests
{
    #region MapQueueIdToGameMode Tests

    [Theory]
    [InlineData(420, "Ranked Solo")]
    [InlineData(440, "Ranked Flex")]
    [InlineData(400, "Normal")]
    [InlineData(430, "Normal")]
    [InlineData(480, "Swift Play")]
    [InlineData(450, "ARAM")]
    [InlineData(2400, "ARAM Mayhem")]
    [InlineData(900, "ARURF")]
    [InlineData(1900, "URF")]
    [InlineData(1700, "Arena")]
    [InlineData(999, "Other")]
    [InlineData(0, "Other")]
    public void MapQueueIdToGameMode_ReturnsCorrectMode(int queueId, string expected)
    {
        var result = DataMapper.MapQueueIdToGameMode(queueId);
        Assert.Equal(expected, result);
    }

    #endregion

    #region ParseDivision Tests (using reflection since it's private)

    [Theory]
    [InlineData("I", 1)]
    [InlineData("II", 2)]
    [InlineData("III", 3)]
    [InlineData("IV", 4)]
    [InlineData("V", 4)]   // Invalid = defaults to 4
    [InlineData(null, 4)]  // Null = defaults to 4
    [InlineData("", 4)]    // Empty = defaults to 4
    public void ParseDivision_ReturnsCorrectValue(string? division, int expected)
    {
        // Use reflection to test private method
        var method = typeof(DataMapper).GetMethod("ParseDivision", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        var result = method?.Invoke(null, new object?[] { division });
        
        Assert.Equal(expected, result);
    }

    #endregion

    #region NormalizePosition Tests

    [Theory]
    [InlineData("TOP", "Top")]
    [InlineData("top", "Top")]
    [InlineData("JUNGLE", "Jungle")]
    [InlineData("jungle", "Jungle")]
    [InlineData("MIDDLE", "Mid")]
    [InlineData("MID", "Mid")]
    [InlineData("mid", "Mid")]
    [InlineData("BOTTOM", "ADC")]
    [InlineData("BOT", "ADC")]
    [InlineData("ADC", "ADC")]
    [InlineData("CARRY", "ADC")]
    [InlineData("UTILITY", "Support")]
    [InlineData("SUPPORT", "Support")]
    [InlineData("support", "Support")]
    [InlineData("NONE", null)]
    [InlineData("NULL", null)]
    [InlineData("UNKNOWN", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData("INVALID", null)]
    public void NormalizePosition_ReturnsCorrectPosition(string? position, string? expected)
    {
        var method = typeof(DataMapper).GetMethod("NormalizePosition",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method?.Invoke(null, new object?[] { position });

        Assert.Equal(expected, result);
    }

    #endregion

    #region IsCommonAdc Tests

    [Theory]
    [InlineData(51, true)]   // Caitlyn
    [InlineData(222, true)]  // Jinx
    [InlineData(67, true)]   // Vayne
    [InlineData(81, true)]   // Ezreal
    [InlineData(119, true)]  // Draven
    [InlineData(145, true)]  // Kai'Sa
    [InlineData(1, false)]   // Annie - not ADC
    [InlineData(99, false)]  // Lux - not ADC
    [InlineData(0, false)]   // Invalid champion
    public void IsCommonAdc_IdentifiesAdcCorrectly(int championId, bool expected)
    {
        var method = typeof(DataMapper).GetMethod("IsCommonAdc",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method?.Invoke(null, new object[] { championId });

        Assert.Equal(expected, result);
    }

    #endregion

    #region IsNoneOrEmpty Tests

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("NONE", true)]
    [InlineData("none", true)]
    [InlineData("None", true)]
    [InlineData("  ", true)]
    [InlineData("Jinx", false)]
    [InlineData("Vayne", false)]
    public void IsNoneOrEmpty_DetectsEmptyValues(string? value, bool expected)
    {
        var method = typeof(DataMapper).GetMethod("IsNoneOrEmpty",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method?.Invoke(null, new object?[] { value });

        Assert.Equal(expected, result);
    }

    #endregion
}
