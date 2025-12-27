using LolStatsTracker.Helpers;
using LolStatsTracker.Services.LeagueAssetsService;
using LolStatsTracker.Services.MatchService;
using LolStatsTracker.Services.MilestoneService;
using LolStatsTracker.Services.SeasonState;
using LolStatsTracker.Services.StatsService;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Helpers;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LolStatsTracker.Pages;

public partial class Dashboard : IDisposable
{
    [Inject] private IStatsService StatsService { get; set; } = null!;
    [Inject] private ILeagueAssetsService LeagueAssetsService { get; set; } = null!;
    [Inject] private IMatchService MatchService { get; set; } = null!;
    [Inject] private IMilestoneService MilestoneService { get; set; } = null!;
    [Inject] private UserProfileState UserState { get; set; } = null!;
    [Inject] private SeasonState SeasonState { get; set; } = null!;

    private StatsSummaryDto? _stats;
    private List<List<(DateTime Date, int Count)>> _matrix = new();
    private int _maxGamesPerDay;
    private List<ChartSeries> _lpSeries = new();
    private string[] _lpLabels = Array.Empty<string>();
    private List<RankMilestoneDto> _milestones = new();

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
        
        // Load rank milestones
        _milestones = await MilestoneService.GetMilestonesAsync();
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
        var labels = new List<string>();

        var i = 1;
        foreach (var m in matches)
        {
            // Calculate total LP across all tiers/divisions for continuous chart
            var totalLp = RankHelper.CalculateTotalLp(m.CurrentTier, m.CurrentDivision, m.CurrentLp);
            data.Add(totalLp);
            labels.Add(i++.ToString());
        }

        _lpSeries.Add(new ChartSeries { Name = "Total LP", Data = data.ToArray() });
        _lpLabels = labels.ToArray();
    }

    private Severity GetTiltSeverity(TiltLevel level) => level switch
    {
        TiltLevel.Critical => Severity.Error,
        TiltLevel.Danger => Severity.Warning,
        TiltLevel.Warning => Severity.Info,
        _ => Severity.Normal
    };

    private static string GetDivisionRoman(int division) => division switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        4 => "IV",
        _ => ""
    };

    public void Dispose()
    {
        UserState.OnProfileChanged -= OnProfileChangedAsync;
        SeasonState.OnSeasonChanged -= OnSeasonChangedAsync;
    }
}
