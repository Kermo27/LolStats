using System.Net.Http.Json;
using Blazored.LocalStorage;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.SeasonState;

public class SeasonState : ISeasonState, IDisposable
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _disposed;

    public event Action? OnChange;
    
    public event Func<Task>? OnSeasonChanged;

    public Season? CurrentSeason { get; private set; }
    public List<Season> AllSeasons { get; private set; } = new();
    public bool IsInitialized { get; private set; }
    
    public bool IsFilteringEnabled => CurrentSeason != null;

    public SeasonState(HttpClient http, ILocalStorageService localStorage)
    {
        _http = http;
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (IsInitialized) return;

            AllSeasons = await _http.GetFromJsonAsync<List<Season>>("api/Seasons") ?? new();

            var savedSeasonId = await _localStorage.GetItemAsync<int?>("selectedSeasonId");
            
            if (savedSeasonId.HasValue)
            {
                CurrentSeason = AllSeasons.FirstOrDefault(s => s.Id == savedSeasonId.Value);
            }
            
            CurrentSeason ??= AllSeasons.FirstOrDefault(s => s.ContainsDate(DateTime.Today));

            IsInitialized = true;
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SeasonState] Error: {ex.Message}");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task SetActiveSeasonAsync(Season? season)
    {
        var changed = CurrentSeason?.Id != season?.Id;
        CurrentSeason = season;
        
        if (season != null)
        {
            await _localStorage.SetItemAsync("selectedSeasonId", season.Id);
        }
        else
        {
            await _localStorage.RemoveItemAsync("selectedSeasonId");
        }
        
        NotifyStateChanged();

        if (changed)
        {
            await NotifySeasonChangedAsync();
        }
    }

    public async Task ClearSeasonFilterAsync()
    {
        await SetActiveSeasonAsync(null);
    }

    public bool IsDateInCurrentSeason(DateTime date)
    {
        if (CurrentSeason == null) return true;
        return CurrentSeason.ContainsDate(date);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    private async Task NotifySeasonChangedAsync()
    {
        if (OnSeasonChanged != null)
        {
            foreach (var handler in OnSeasonChanged.GetInvocationList().Cast<Func<Task>>())
            {
                await handler();
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _initLock.Dispose();
            _disposed = true;
        }
    }
}
