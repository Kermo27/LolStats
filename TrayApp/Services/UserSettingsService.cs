using System.IO;
using System.Text.Json;
using LolStatsTracker.TrayApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LolStatsTracker.TrayApp.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly ILogger<UserSettingsService> _logger;
    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;
    private readonly string _appExecutablePath;
    private UserSettings _settings = new();
    
    private const string AppName = "LolStatsTracker";
    private const string SettingsFileName = "usersettings.json";
    private const string RegistryRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public UserSettings Settings => _settings.Clone();
    
    public event EventHandler<UserSettings>? SettingsChanged;
    
    public UserSettingsService(ILogger<UserSettingsService> logger)
    {
        _logger = logger;
        
        // Settings directory: %APPDATA%\LolStatsTracker\
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsDirectory = Path.Combine(appData, AppName);
        _settingsFilePath = Path.Combine(_settingsDirectory, SettingsFileName);
        
        // Get executable path for auto-start
        _appExecutablePath = Environment.ProcessPath ?? 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{AppName}.TrayApp.exe");
        
        _logger.LogInformation("Settings file path: {Path}", _settingsFilePath);
    }
    
    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);
                
                if (loaded != null)
                {
                    _settings = loaded;
                    _logger.LogInformation("Settings loaded from {Path}", _settingsFilePath);
                    
                    // Sync auto-start registry with loaded setting
                    SyncAutoStartRegistry(_settings.AutoStartWithWindows);
                    return;
                }
            }
            
            _logger.LogInformation("No settings file found, using defaults");
            _settings = new UserSettings();
            
            // Save defaults so user can see the file
            await SaveToFileAsync(_settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings, using defaults");
            _settings = new UserSettings();
        }
    }
    
    public async Task SaveAsync(UserSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        
        var previousSettings = _settings;
        _settings = settings.Clone();
        
        await SaveToFileAsync(_settings);
        
        // Handle auto-start change
        if (previousSettings.AutoStartWithWindows != _settings.AutoStartWithWindows)
        {
            SyncAutoStartRegistry(_settings.AutoStartWithWindows);
        }
        
        _logger.LogInformation("Settings saved successfully");
        
        // Notify subscribers (for hot-reload)
        SettingsChanged?.Invoke(this, _settings.Clone());
    }
    
    private async Task SaveToFileAsync(UserSettings settings)
    {
        try
        {
            // Ensure directory exists
            if (!Directory.Exists(_settingsDirectory))
            {
                Directory.CreateDirectory(_settingsDirectory);
                _logger.LogInformation("Created settings directory: {Path}", _settingsDirectory);
            }
            
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings to file");
            throw;
        }
    }
    
    private void SyncAutoStartRegistry(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, writable: true);
            
            if (key == null)
            {
                _logger.LogWarning("Could not open registry key: {Key}", RegistryRunKey);
                return;
            }
            
            if (enable)
            {
                key.SetValue(AppName, $"\"{_appExecutablePath}\"");
                _logger.LogInformation("Auto-start enabled in registry");
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
                _logger.LogInformation("Auto-start disabled in registry");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update auto-start registry");
        }
    }
}
