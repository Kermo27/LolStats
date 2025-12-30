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

public class LpDataPoint
{
    public int GameNumber { get; set; }
    public int TotalLp { get; set; }
    public string RankDisplay { get; set; } = "";
    public string Champion { get; set; } = "";
    public bool Win { get; set; }
}

public partial class Dashboard : IDisposable
{
    [Inject] private IStatsService StatsService { get; set; } = null!;
    [Inject] private ILeagueAssetsService LeagueAssetsService { get; set; } = null!;
    [Inject] private IMatchService MatchService { get; set; } = null!;
    [Inject] private IMilestoneService MilestoneService { get; set; } = null!;
    [Inject] private IUserProfileState UserState { get; set; } = null!;
    [Inject] private ISeasonState SeasonState { get; set; } = null!;

    private StatsSummaryDto? _stats;
    private List<List<(DateTime Date, int Count)>> _matrix = new();
    private int _maxGamesPerDay;
    private List<RankMilestoneDto> _milestones = new();
    private (string Start, string End, int Gained)? _lpRangeInfo;
    
    private string _selectedGameMode = "Ranked Solo";
    private string _selectedRole = "All";
    private List<EnemyStatsDto> _hardestEnemies = new();
    private List<DuoSummary> _bestDuos = new();
    
    private List<LpDataPoint> _lpChartData = new();
    private List<ChartSeries> _lpSeries = new();
    private string[] _lpLabels = Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
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

    private async Task OnGameModeChanged(string mode)
    {
        _selectedGameMode = mode;
        await LoadDashboardDataAsync();
    }

    private async Task OnRoleChanged(string role)
    {
        _selectedRole = role;
        await LoadHardestEnemiesAsync();
        await LoadBestDuosAsync();
        StateHasChanged();
    }

    private List<ChampionStatsDto> FilteredChampions => _stats?.ChampionStats
        .Where(c => _selectedRole == "All" || c.Role == _selectedRole)
        .ToList() ?? new();

    private async Task LoadHardestEnemiesAsync()
    {
        var seasonId = SeasonState.CurrentSeason?.Id;
        var gameModeFilter = _selectedGameMode == "All" ? null : _selectedGameMode;
        _hardestEnemies = await StatsService.GetHardestEnemiesAsync(_selectedRole, seasonId, gameModeFilter);
    }

    private async Task LoadBestDuosAsync()
    {
        var seasonId = SeasonState.CurrentSeason?.Id;
        var gameModeFilter = _selectedGameMode == "All" ? null : _selectedGameMode;
        _bestDuos = await StatsService.GetBestDuosAsync(_selectedRole, seasonId, gameModeFilter);
    }

    private async Task LoadDashboardDataAsync()
    {
        if (UserState.CurrentProfile == null)
        {
            Console.WriteLine("Dashboard: No profile selected.");
            _stats = null;
            return;
        }
        
        _lpChartData = new List<LpDataPoint>();
        _lpSeries = new List<ChartSeries>();
        _lpLabels = Array.Empty<string>();
        
        var seasonId = SeasonState.CurrentSeason?.Id;
        var gameModeFilter = _selectedGameMode == "All" ? null : _selectedGameMode;
        _stats = await StatsService.GetSummaryAsync(6, seasonId, gameModeFilter);
        
        var allMatches = await MatchService.GetAllAsync();
        var matches = allMatches
            .Where(m => SeasonState.IsDateInCurrentSeason(m.Date))
            .Where(m => _selectedGameMode == "All" || m.GameMode == _selectedGameMode)
            .OrderBy(m => m.Date)
            .TakeLast(20)
            .ToList();
        
        if (_stats != null)
        {
            PrepareActivityMatrix(_stats.Activity);
            PrepareLpChart(matches);
        }
        
        _milestones = await MilestoneService.GetMilestonesAsync();
        await LoadHardestEnemiesAsync();
        await LoadBestDuosAsync();
    }

    private void PrepareActivityMatrix(List<ActivityDayDto> activity)
    {
        var builder = new ActivityMatrixBuilder(activity);
        _matrix = builder.Build();
        
        _maxGamesPerDay = activity.Any() ? activity.Max(x => x.GamesPlayed) : 1; 
    }
    
    private void PrepareLpChart(List<MatchEntry> matches)
    {
        _lpChartData = new List<LpDataPoint>();
        var data = new List<double>();
        var labels = new List<string>();

        var i = 1;
        foreach (var m in matches)
        {
            var totalLp = RankHelper.CalculateTotalLp(m.CurrentTier, m.CurrentDivision, m.CurrentLp);
            
            _lpChartData.Add(new LpDataPoint
            {
                GameNumber = i,
                TotalLp = totalLp,
                RankDisplay = RankHelper.FormatRank(m.CurrentTier, m.CurrentDivision, m.CurrentLp),
                Champion = m.Champion,
                Win = m.Win
            });
            
            data.Add(totalLp);
            labels.Add(i++.ToString());
        }
        
        _lpSeries.Add(new ChartSeries { Name = "LP", Data = data.ToArray() });
        _lpLabels = labels.ToArray();
        
        if (matches.Count >= 2)
        {
            var first = matches.First();
            var last = matches.Last();
            var startLp = RankHelper.CalculateTotalLp(first.CurrentTier, first.CurrentDivision, first.CurrentLp);
            var endLp = RankHelper.CalculateTotalLp(last.CurrentTier, last.CurrentDivision, last.CurrentLp);
            
            _lpRangeInfo = (
                Start: RankHelper.FormatRank(first.CurrentTier, first.CurrentDivision, first.CurrentLp),
                End: RankHelper.FormatRank(last.CurrentTier, last.CurrentDivision, last.CurrentLp),
                Gained: endLp - startLp
            );
        }
        else
        {
            _lpRangeInfo = null;
        }
    }

    private Severity GetTiltSeverity(TiltLevel level) => level switch
    {
        TiltLevel.Critical => Severity.Error,
        TiltLevel.Danger => Severity.Warning,
        TiltLevel.Warning => Severity.Info,
        _ => Severity.Normal
    };

    public void Dispose()
    {
        UserState.OnProfileChanged -= OnProfileChangedAsync;
        SeasonState.OnSeasonChanged -= OnSeasonChangedAsync;
    }
}


