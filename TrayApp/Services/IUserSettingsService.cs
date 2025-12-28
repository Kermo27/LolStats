using LolStatsTracker.TrayApp.Models;

namespace LolStatsTracker.TrayApp.Services;

public interface IUserSettingsService
{
    UserSettings Settings { get; }
    event EventHandler<UserSettings>? SettingsChanged;
    Task LoadAsync();
    Task SaveAsync(UserSettings settings);
}
