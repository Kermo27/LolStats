namespace LolStatsTracker.TrayApp.Models;

public class UserSettings
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5031";
    public int CheckIntervalSeconds { get; set; } = 10;
    public bool AutoStartWithWindows { get; set; } = false;
    
    // Riot API Settings
    public string? RiotApiKey { get; set; }
    public string RiotRegion { get; set; } = "euw1"; // euw1, na1, kr, etc.
    
    public UserSettings Clone() => new()
    {
        ApiBaseUrl = ApiBaseUrl,
        CheckIntervalSeconds = CheckIntervalSeconds,
        AutoStartWithWindows = AutoStartWithWindows,
        RiotApiKey = RiotApiKey,
        RiotRegion = RiotRegion
    };
}
