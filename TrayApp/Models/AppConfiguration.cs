namespace LolStatsTracker.TrayApp.Models;

public class AppConfiguration
{
    public string ApiBaseUrl { get; set; } = "https://localhost:7045";
    public Guid ProfileId { get; set; } = Guid.Empty;
    public bool AutoStartWithWindows { get; set; } = false;
    public int CheckIntervalSeconds { get; set; } = 10;
}
