namespace LolStatsTracker.TrayApp.Models;

public class LcuConnectionInfo
{
    public int ProcessId { get; set; }
    public int Port { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Protocol { get; set; } = "https";
    
    public string BaseUrl => $"{Protocol}://127.0.0.1:{Port}";
    public string WebSocketUrl => $"wss://127.0.0.1:{Port}/";
    
    public string GetBasicAuthToken()
    {
        var credentials = $"riot:{Password}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(credentials);
        return Convert.ToBase64String(bytes);
    }
}
