using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace LolStatsTracker.TrayApp.Services;

public class UpdateService(ILogger<UpdateService> logger)
{
    private readonly ILogger<UpdateService> _logger = logger;
    private UpdateManager? _updateManager;

    public async Task InitializeAsync()
    {
        await Task.Yield();
        try
        {
            _updateManager = new UpdateManager(new GithubSource("https://github.com/Kermo27/LolStats", null, true));
            _logger.LogInformation("UpdateService initialized.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize UpdateService.");
        }
    }

    public async Task CheckForUpdatesAsync()
    {
        if (_updateManager == null) return;

        try
        {
            _logger.LogInformation("Checking for updates...");
            var updateInfo = await _updateManager.CheckForUpdatesAsync();

            if (updateInfo == null)
            {
                _logger.LogInformation("No updates available.");
                return;
            }

            _logger.LogInformation("Update found: {Version}", updateInfo.TargetFullRelease.Version);

            await _updateManager.DownloadUpdatesAsync(updateInfo);
            _logger.LogInformation("Update downloaded.");
            
            _updateManager.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check/download updates.");
        }
    }
}
