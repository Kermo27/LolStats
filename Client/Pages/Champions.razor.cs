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
    [Inject] private IUserProfileState UserState { get; set; } = null!;
    [Inject] private ISeasonState SeasonState { get; set; } = null!;

    private List<ChampionStatsDto> _championStats = new();
    private bool _isLoading = true;
    private string _searchString = "";
    private string _selectedRole = "All";
    private string _selectedGameMode = "Ranked Solo";

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

    private async Task OnGameModeChanged(string mode)
    {
        _selectedGameMode = mode;
        _isLoading = true;
        StateHasChanged();
        await LoadDataAsync();
        StateHasChanged();
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
        var gameModeFilter = _selectedGameMode == "All" ? null : _selectedGameMode;
        _championStats = await StatsService.GetChampionsAsync(seasonId, gameModeFilter);
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
    
    private static Color GetRoleColor(string role) => role switch
    {
        "Top" => Color.Warning,
        "Jungle" => Color.Success,
        "Mid" => Color.Info,
        "ADC" => Color.Error,
        "Support" => Color.Secondary,
        _ => Color.Default
    };
    
    private static string GetKdaClass(double kda)
    {
        if (kda >= 4.0) return "text-success font-weight-bold";
        if (kda >= 2.5) return "text-info";
        return "text-error";
    }
    
    private bool FilterFunc(ChampionStatsDto element)
    {
        if (_selectedRole != "All" && element.Role != _selectedRole) return false;
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
