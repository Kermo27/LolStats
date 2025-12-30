using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.UserState;

public interface IUserProfileState
{
    event Action? OnChange;
    event Func<Task>? OnProfileChanged;
    
    UserProfile? CurrentProfile { get; }
    List<UserProfile> AllProfiles { get; }
    bool IsInitialized { get; }
    
    Task InitializeAsync();
    Task SetActiveProfileAsync(UserProfile profile);
    Task AddProfile(string name, string tag);
    Task UpdateProfile(UserProfile profile);
    Task DeleteProfile(Guid id);
}
