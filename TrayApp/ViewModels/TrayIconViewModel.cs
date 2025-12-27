using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows;
using LolStatsTracker.TrayApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LolStatsTracker.TrayApp.ViewModels;

public partial class TrayIconViewModel : ObservableObject
{
    private readonly LcuService _lcuService;
    private readonly TrayBackgroundService _backgroundService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _statusText = "Starting...";

    [ObservableProperty]
    private string _toolTipText = "LoL Stats Tracker";

    public TrayIconViewModel(
        LcuService lcuService, 
        TrayBackgroundService backgroundService,
        IServiceProvider serviceProvider)
    {
        _lcuService = lcuService;
        _backgroundService = backgroundService;
        _serviceProvider = serviceProvider;

        _backgroundService.StatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(object? sender, string status)
    {
        StatusText = $"Status: {status}";
        ToolTipText = $"LoL Stats Tracker - {status}";
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settingsWindow = _serviceProvider.GetRequiredService<Views.SettingsWindow>();
        var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
        
        settingsWindow.DataContext = settingsViewModel;
        settingsViewModel.CloseAction = settingsWindow.Close;
        
        settingsWindow.Show();
    }

    [RelayCommand]
    private void OpenDashboard()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://localhost:5067",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open dashboard: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }
}
