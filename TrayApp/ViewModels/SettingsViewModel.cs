using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Services;
using Microsoft.Extensions.Options;

namespace LolStatsTracker.TrayApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppConfiguration _config;
    // In a real app we'd have a SettingsService to persist changes. 
    // For now we just bind to current config (in-memory) or mock saving.

    [ObservableProperty]
    private string _serverUrl;

    [ObservableProperty]
    private int _checkInterval;

    public SettingsViewModel(IOptions<AppConfiguration> config)
    {
        _config = config.Value;
        _serverUrl = _config.ApiBaseUrl;
        _checkInterval = _config.CheckIntervalSeconds;
    }

    [RelayCommand]
    private void Save()
    {
        // Here we would save to appsettings.json or user settings
        // For simplicity, we assume this is wired up
        System.Windows.MessageBox.Show("Settings saved (simulation). Restart required for some changes.");
        CloseAction?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseAction?.Invoke();
    }

    public Action? CloseAction { get; set; }
}
