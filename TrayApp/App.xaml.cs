using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Services;
using LolStatsTracker.TrayApp.ViewModels;
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
    private TrayAuthService? _authService;
    private TrayIconViewModel? _trayIconViewModel;
    
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
                
                // Settings Service (persistent user settings)
                services.AddSingleton<IUserSettingsService, UserSettingsService>();
                
                // Services
                services.AddSingleton<TrayAuthService>();
                services.AddSingleton<LcuService>();
                services.AddHttpClient<ApiSyncService>();
                
                services.AddSingleton<TrayBackgroundService>();
                services.AddHostedService(provider => provider.GetRequiredService<TrayBackgroundService>());
                
                // ViewModels
                services.AddSingleton<TrayIconViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<SettingsViewModel>();
                
                // Views
                services.AddTransient<LoginWindow>();
                services.AddTransient<SettingsWindow>();
                
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });
            })
            .Build();
        
        // Load user settings
        var settingsService = _host.Services.GetRequiredService<IUserSettingsService>();
        await settingsService.LoadAsync();
        
        // Authenticate
        _authService = _host.Services.GetRequiredService<TrayAuthService>();
        var isAuthenticated = await _authService.TryLoadStoredTokenAsync();
        
        if (!isAuthenticated)
        {
            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            var loginVm = _host.Services.GetRequiredService<LoginViewModel>();
            
            // Wire up VM to View manually since we don't have a robust VM-First navigation system yet
            loginWindow.DataContext = loginVm;
            
            loginVm.LoginResultAction = (success) => 
            {
                if (success)
                {
                    loginWindow.LoginSuccessful = true;
                    loginWindow.DialogResult = true;
                    loginWindow.Close();
                }
            };
            
            var result = loginWindow.ShowDialog();
            
            if (result != true)
            {
                Shutdown();
                return;
            }
        }
        
        await _host.StartAsync();
        
        SetupTrayIcon();
    }
    
    private void SetupTrayIcon()
    {
        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _trayIconViewModel = _host!.Services.GetRequiredService<TrayIconViewModel>();
        
        if (_trayIcon != null)
        {
            // DataBind ViewModel to TrayIcon
            _trayIcon.DataContext = _trayIconViewModel;
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
    
    // Override OnExit to ensure clean shutdown
    protected override async void OnExit(ExitEventArgs e)
    {
        await ExitApplication();
        base.OnExit(e);
    }
}

