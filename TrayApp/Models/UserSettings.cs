namespace LolStatsTracker.TrayApp.Models;

public class UserSettings
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5031";
    public int CheckIntervalSeconds { get; set; } = 10;
    public bool AutoStartWithWindows { get; set; } = false;
    
    public UserSettings Clone() => new()
    {
        ApiBaseUrl = ApiBaseUrl,
        CheckIntervalSeconds = CheckIntervalSeconds,
        AutoStartWithWindows = AutoStartWithWindows
    };
}
