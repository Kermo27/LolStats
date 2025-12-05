using LolStatsTracker.Helpers;
using LolStatsTracker.Services.LeagueAssetsService;
using LolStatsTracker.Services.MatchService;
using LolStatsTracker.Services.SeasonState;
using LolStatsTracker.Services.StatsService;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LolStatsTracker.Pages;

public partial class Dashboard : IDisposable
{
    [Inject] private IStatsService StatsService { get; set; } = null!;
    [Inject] private ILeagueAssetsService LeagueAssetsService { get; set; } = null!;
    [Inject] private IMatchService MatchService { get; set; } = null!;
    [Inject] private UserProfileState UserState { get; set; } = null!;
    [Inject] private SeasonState SeasonState { get; set; } = null!;

    private StatsSummaryDto? _stats;
    private List<List<(DateTime Date, int Count)>> _matrix = new();
    private int _maxGamesPerDay;
    private List<ChartSeries> _lpSeries = new();
    private string[] _lpLabels = Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to profile and season changes
        UserState.OnProfileChanged += OnProfileChangedAsync;
        SeasonState.OnSeasonChanged += OnSeasonChangedAsync;
        
        await LeagueAssetsService.InitializeAsync();
        
        if (!UserState.IsInitialized) 
            await UserState.InitializeAsync();
        
        if (!SeasonState.IsInitialized)
            await SeasonState.InitializeAsync();
        
        await LoadDashboardDataAsync();
    }

    private async Task OnProfileChangedAsync()
    {
        await LoadDashboardDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnSeasonChangedAsync()
    {
        await LoadDashboardDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadDashboardDataAsync()
    {
        if (UserState.CurrentProfile == null)
        {
            Console.WriteLine("Dashboard: No profile selected.");
            _stats = null;
            return;
        }
        
        // Reset chart data
        _lpSeries = new List<ChartSeries>();
        _lpLabels = Array.Empty<string>();
        
        // Get season filtered stats from server
        var seasonId = SeasonState.CurrentSeason?.Id;
        _stats = await StatsService.GetSummaryAsync(6, seasonId);
        
        // Get matches and filter by season
        var allMatches = await MatchService.GetAllAsync();
        var matches = allMatches
            .Where(m => SeasonState.IsDateInCurrentSeason(m.Date))
            .OrderBy(m => m.Date)
            .TakeLast(20)
            .ToList();
        
        if (_stats != null)
        {
            PrepareActivityMatrix(_stats.Activity);
            PrepareLpChart(matches);
        }
    }

    private void PrepareActivityMatrix(List<ActivityDayDto> activity)
    {
        var builder = new ActivityMatrixBuilder(activity);
        _matrix = builder.Build();
        
        _maxGamesPerDay = activity.Any() ? activity.Max(x => x.GamesPlayed) : 1; 
    }
    
    private string GetHeatmapColor(int gameCount)
    {
        if (gameCount == 0) return "#282830"; 

        var normalized = (double)gameCount / _maxGamesPerDay;

        return normalized switch
        {
            >= 0.75 => "#594ae2",
            >= 0.40 => "#4840b0",
            _ => "#3f3c6e"
        };
    }
    
    private void PrepareLpChart(List<MatchEntry> matches)
    {
        var data = new List<double>();
        double currentNet = 0;
        var labels = new List<string>();

        var i = 1;
        foreach (var m in matches)
        {
            currentNet += m.LpChange;
            data.Add(currentNet);
            labels.Add(i++.ToString());
        }

        _lpSeries.Add(new ChartSeries { Name = "Net LP Change", Data = data.ToArray() });
        _lpLabels = labels.ToArray();
    }

    public void Dispose()
    {
        UserState.OnProfileChanged -= OnProfileChangedAsync;
        SeasonState.OnSeasonChanged -= OnSeasonChangedAsync;
    }
}
