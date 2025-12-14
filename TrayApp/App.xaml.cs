using System.Diagnostics;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Services;
using LolStatsTracker.TrayApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LolStatsTracker.TrayApp;

public partial class App : Application
{
    private IHost? _host;
    private TaskbarIcon? _trayIcon;
    
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppConfiguration>(context.Configuration.GetSection("AppConfiguration"));
                
                services.AddSingleton<LcuConnectionManager>();
                services.AddSingleton<LcuApiClient>();
                services.AddSingleton<LcuEventListener>();
                services.AddHttpClient<ApiSyncService>();
                services.AddHostedService<TrayBackgroundService>();
                
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });
            })
            .Build();
        
        await _host.StartAsync();
        
        SetupTrayIcon();
    }
    
    private void SetupTrayIcon()
    {
        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        
        if (_trayIcon?.ContextMenu != null)
        {
            var statusMenuItem = _trayIcon.ContextMenu.Items[0] as System.Windows.Controls.MenuItem;
            var settingsMenuItem = _trayIcon.ContextMenu.Items[2] as System.Windows.Controls.MenuItem;
            var dashboardMenuItem = _trayIcon.ContextMenu.Items[3] as System.Windows.Controls.MenuItem;
            var exitMenuItem = _trayIcon.ContextMenu.Items[5] as System.Windows.Controls.MenuItem;
            
            if (settingsMenuItem != null)
                settingsMenuItem.Click += (s, e) => OpenSettings();
            
            if (dashboardMenuItem != null)
                dashboardMenuItem.Click += (s, e) => OpenDashboard();
            
            if (exitMenuItem != null)
                exitMenuItem.Click += async (s, e) => await ExitApplication();
            
            var backgroundService = _host?.Services.GetService<IHostedService>() as TrayBackgroundService;
            if (backgroundService != null)
            {
                backgroundService.StatusChanged += (s, status) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (statusMenuItem != null)
                            statusMenuItem.Header = $"Status: {status}";
                        
                        if (_trayIcon != null)
                            _trayIcon.ToolTipText = $"LoL Stats Tracker - {status}";
                    });
                };
            }
        }
    }
    
    private void OpenSettings()
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Show();
    }
    
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
            MessageBox.Show($"Could not open dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async Task ExitApplication()
    {
        _trayIcon?.Dispose();
        
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        
        Shutdown();
    }
}
