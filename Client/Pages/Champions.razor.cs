using LolStatsTracker.Services.LeagueAssetsService;
using LolStatsTracker.Services.SeasonState;
using LolStatsTracker.Services.StatsService;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LolStatsTracker.Pages;

public partial class Champions : IDisposable
{
    [Inject] private IStatsService StatsService { get; set; } = null!;
    [Inject] private ILeagueAssetsService LeagueAssetsService { get; set; } = null!;
    [Inject] private UserProfileState UserState { get; set; } = null!;
    [Inject] private SeasonState SeasonState { get; set; } = null!;

    private List<ChampionStatsDto> _championStats = new();
    private bool _isLoading = true;
    private string _searchString = "";

    protected override async Task OnInitializedAsync()
    {
        UserState.OnProfileChanged += OnProfileChangedAsync;
        SeasonState.OnSeasonChanged += OnSeasonChangedAsync;
        
        await LeagueAssetsService.InitializeAsync();
        
        if (!UserState.IsInitialized)
            await UserState.InitializeAsync();
        
        if (!SeasonState.IsInitialized)
            await SeasonState.InitializeAsync();
            
        await LoadDataAsync();
    }

    private async Task OnProfileChangedAsync()
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        
        await LoadDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnSeasonChangedAsync()
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        
        await LoadDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadDataAsync()
    {
        if (UserState.CurrentProfile == null)
        {
            _championStats = new List<ChampionStatsDto>();
            _isLoading = false;
            return;
        }
        
        var seasonId = SeasonState.CurrentSeason?.Id;
        _championStats = await StatsService.GetChampionsAsync(seasonId);
        _isLoading = false;
    }

    private static Color GetColor(double wr)
    {
        if (wr >= 0.6) return Color.Success; 
        if (wr >= 0.5) return Color.Info;    
        return Color.Error;                 
    }
    
    private static Color GetWinrateColor(double wr)
    {
        if (wr >= 0.5) return Color.Success;
        return Color.Error;
    }
    
    private bool FilterFunc(ChampionStatsDto element)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;
        if (element.ChampionName.Contains(_searchString, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public void Dispose()
    {
        UserState.OnProfileChanged -= OnProfileChangedAsync;
        SeasonState.OnSeasonChanged -= OnSeasonChangedAsync;
    }
}
