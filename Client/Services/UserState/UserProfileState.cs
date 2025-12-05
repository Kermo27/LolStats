using System.Net.Http.Json;
using Blazored.LocalStorage;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.UserState;

public class UserProfileState : IDisposable
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;

    private readonly SemaphoreSlim _initializationLock = new SemaphoreSlim(1, 1);
    private bool _disposed;

    public event Action? OnChange;

    public event Func<Task>? OnProfileChanged;
    
    public UserProfile? CurrentProfile { get; private set; }
    public List<UserProfile> AllProfiles { get; private set; } = new();
    public bool IsInitialized { get; private set; }

    public UserProfileState(HttpClient http, ILocalStorageService localStorage)
    {
        _http = http;
        _localStorage = localStorage;
    }
    
    public async Task InitializeAsync()
    {
        // Only one thread at a time can enter this block
        await _initializationLock.WaitAsync();

        try
        {
            if (IsInitialized) return;

            AllProfiles = await _http.GetFromJsonAsync<List<UserProfile>>("api/Profiles") ?? new();

            if (!AllProfiles.Any())
            {
                var defaultProfile = new UserProfile { Name = "Main Account", Tag = "EUW", IsDefault = true };
                var response = await _http.PostAsJsonAsync("api/Profiles", defaultProfile);
                if (response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<UserProfile>();
                    if (created != null) AllProfiles.Add(created);
                }
            }

            var savedId = await _localStorage.GetItemAsync<string>("selectedProfileId");

            if (!string.IsNullOrEmpty(savedId) && Guid.TryParse(savedId, out var guid))
            {
                CurrentProfile = AllProfiles.FirstOrDefault(p => p.Id == guid);
            }

            if (CurrentProfile == null && AllProfiles.Any())
            {
                CurrentProfile = AllProfiles.First();

                await _localStorage.SetItemAsync("selectedProfileId", CurrentProfile.Id.ToString());
            }

            IsInitialized = true;
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching profiles: {ex.Message}");
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task SetActiveProfileAsync(UserProfile profile)
    {
        var profileChanged = CurrentProfile?.Id != profile.Id;
        CurrentProfile = profile;
        await _localStorage.SetItemAsync("selectedProfileId", profile.Id.ToString());
        NotifyStateChanged();
        
        if (profileChanged)
        {
            await NotifyProfileChangedAsync();
        }
    }

    public async Task AddProfile(string name, string tag)
    {
        var newProfile = new UserProfile { Name = name, Tag = tag };
        var res = await _http.PostAsJsonAsync("api/Profiles", newProfile);
        
        if (res.IsSuccessStatusCode)
        {
            var created = await res.Content.ReadFromJsonAsync<UserProfile>();
            if (created != null)
            {
                AllProfiles.Add(created);
                await SetActiveProfileAsync(created);
            }
        }
    }
    
    public async Task UpdateProfile(UserProfile profile)
    {
        var response = await _http.PutAsJsonAsync($"api/profiles/{profile.Id}", profile);
        if (response.IsSuccessStatusCode)
        {
            var index = AllProfiles.FindIndex(p => p.Id == profile.Id);
            if (index != -1)
            {
                AllProfiles[index] = profile;
                
                if (CurrentProfile?.Id == profile.Id)
                {
                    CurrentProfile = profile;
                }
                NotifyStateChanged();
            }
        }
    }
    
    public async Task DeleteProfile(Guid id)
    {
        var response = await _http.DeleteAsync($"api/profiles/{id}");
        if (response.IsSuccessStatusCode)
        {
            var profileToRemove = AllProfiles.FirstOrDefault(p => p.Id == id);
            if (profileToRemove != null)
            {
                AllProfiles.Remove(profileToRemove);
                
                if (CurrentProfile?.Id == id)
                {
                    var fallback = AllProfiles.FirstOrDefault();
                    if (fallback != null)
                    {
                        await SetActiveProfileAsync(fallback);
                    }
                    else
                    {
                        CurrentProfile = null; // Brak profili
                        await _localStorage.RemoveItemAsync("selectedProfileId");
                    }
                }
                NotifyStateChanged();
            }
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
    
    private async Task NotifyProfileChangedAsync()
    {
        if (OnProfileChanged != null)
        {
            foreach (var handler in OnProfileChanged.GetInvocationList().Cast<Func<Task>>())
            {
                await handler();
            }
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _initializationLock.Dispose();
            }
            _disposed = true;
        }
    }
}