using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Services;

namespace LolStatsTracker.TrayApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IUserSettingsService _settingsService;

    [ObservableProperty]
    private string _serverUrl = string.Empty;

    [ObservableProperty]
    private int _checkInterval;

    [ObservableProperty]
    private bool _autoStartWithWindows;

    [ObservableProperty]
    private string _riotApiKey = string.Empty;

    [ObservableProperty]
    private string _riotRegion = "euw1";

    [ObservableProperty]
    private bool _isSaving;

    public SettingsViewModel(IUserSettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        var settings = _settingsService.Settings;
        ServerUrl = settings.ApiBaseUrl;
        CheckInterval = settings.CheckIntervalSeconds;
        AutoStartWithWindows = settings.AutoStartWithWindows;
        RiotApiKey = settings.RiotApiKey ?? string.Empty;
        RiotRegion = settings.RiotRegion;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsSaving) return;
        
        try
        {
            IsSaving = true;
            
            var settings = new UserSettings
            {
                ApiBaseUrl = ServerUrl,
                CheckIntervalSeconds = CheckInterval,
                AutoStartWithWindows = AutoStartWithWindows,
                RiotApiKey = string.IsNullOrWhiteSpace(RiotApiKey) ? null : RiotApiKey,
                RiotRegion = RiotRegion
            };
            
            await _settingsService.SaveAsync(settings);
            
            System.Windows.MessageBox.Show(
                "Settings saved successfully!", 
                "Settings", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Information);
            
            CloseAction?.Invoke();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to save settings: {ex.Message}", 
                "Error", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseAction?.Invoke();
    }

    public Action? CloseAction { get; set; }
}
