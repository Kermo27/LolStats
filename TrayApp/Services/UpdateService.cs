using System.IO;
using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace LolStatsTracker.TrayApp.Services;

public class UpdateService(ILogger<UpdateService> logger)
{
    private readonly ILogger<UpdateService> _logger = logger;
    private UpdateManager? _updateManager;
    private static readonly string LogFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LolStatsTracker", "update.log");

    private static void Log(string message)
    {
        try
        {
            var dir = Path.GetDirectoryName(LogFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";
            File.AppendAllText(LogFile, line);
        }
        catch { }
    }

    public async Task InitializeAsync()
    {
        await Task.Yield();
        try
        {
            Log("Initializing UpdateService...");
            
            _updateManager = new UpdateManager(new GithubSource("https://github.com/Kermo27/LolStats", null, true));
            
            Log($"Current app version: {_updateManager.CurrentVersion?.ToString() ?? "unknown"}");
            Log($"App ID: {_updateManager.AppId ?? "unknown"}");
            Log($"Is installed: {_updateManager.IsInstalled}");
            Log("UpdateService initialized successfully.");
            _logger.LogInformation("UpdateService initialized.");
        }
        catch (Exception ex)
        {
            Log($"Failed to initialize UpdateService: {ex.Message}");
            _logger.LogError(ex, "Failed to initialize UpdateService.");
        }
    }

    public async Task CheckForUpdatesAsync()
    {
        if (_updateManager == null)
        {
            Log("UpdateManager is null, skipping check.");
            return;
        }

        try
        {
            Log("Checking for updates...");
            _logger.LogInformation("Checking for updates...");
            var updateInfo = await _updateManager.CheckForUpdatesAsync();

            if (updateInfo == null)
            {
                Log("No updates available.");
                _logger.LogInformation("No updates available.");
                return;
            }

            Log($"Update found: {updateInfo.TargetFullRelease.Version}");
            _logger.LogInformation("Update found: {Version}", updateInfo.TargetFullRelease.Version);

            Log("Downloading update...");
            await _updateManager.DownloadUpdatesAsync(updateInfo);
            Log("Update downloaded.");
            _logger.LogInformation("Update downloaded.");
            
            Log("Applying update and restarting...");
            _updateManager.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception ex)
        {
            Log($"Failed to check/download updates: {ex.Message}");
            _logger.LogError(ex, "Failed to check/download updates.");
        }
    }
}
