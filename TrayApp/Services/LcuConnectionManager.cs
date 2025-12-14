using System.IO;
using LolStatsTracker.TrayApp.Models;
using Microsoft.Extensions.Logging;

namespace LolStatsTracker.TrayApp.Services;

public class LcuConnectionManager
{
    private readonly ILogger<LcuConnectionManager> _logger;
    private LcuConnectionInfo? _currentConnection;
    
    public event EventHandler<LcuConnectionInfo>? Connected;
    public event EventHandler? Disconnected;
    
    public bool IsConnected => _currentConnection != null;
    public LcuConnectionInfo? CurrentConnection => _currentConnection;
    
    public LcuConnectionManager(ILogger<LcuConnectionManager> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> TryConnectAsync()
    {
        try
        {
            if (!Helpers.ProcessHelper.IsProcessRunning("LeagueClientUx"))
            {
                if (_currentConnection != null)
                {
                    _logger.LogInformation("League Client disconnected");
                    _currentConnection = null;
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }
                return false;
            }
            
            var lockfilePath = @"F:\Riot Games\League of Legends\lockfile";
            
            _logger.LogDebug("Checking lockfile at: {Path}", lockfilePath);
            
            if (!File.Exists(lockfilePath))
            {
                _logger.LogWarning("Lockfile not found at {Path}", lockfilePath);
                return false;
            }
            
            // Parse lockfile
            var lockfileContent = await ReadLockfileAsync(lockfilePath);
            if (string.IsNullOrEmpty(lockfileContent))
                return false;
            
            var parts = lockfileContent.Split(':');
            if (parts.Length != 5)
            {
                _logger.LogWarning("Invalid lockfile format");
                return false;
            }
            
            var connectionInfo = new LcuConnectionInfo
            {
                ProcessId = int.Parse(parts[1]),
                Port = int.Parse(parts[2]),
                Password = parts[3],
                Protocol = parts[4]
            };
            
            // Check if connection info changed
            if (_currentConnection == null || 
                _currentConnection.Port != connectionInfo.Port || 
                _currentConnection.Password != connectionInfo.Password)
            {
                _currentConnection = connectionInfo;
                _logger.LogInformation("LCU connected on port {Port}", connectionInfo.Port);
                Connected?.Invoke(this, connectionInfo);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to LCU");
            return false;
        }
    }
    
    private async Task<string> ReadLockfileAsync(string path)
    {
        try
        {
            // Lockfile might be locked by client, retry mechanism
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(fileStream);
                    return await reader.ReadToEndAsync();
                }
                catch (IOException)
                {
                    if (i == 2) throw;
                    await Task.Delay(100);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read lockfile");
        }
        
        return string.Empty;
    }
}
