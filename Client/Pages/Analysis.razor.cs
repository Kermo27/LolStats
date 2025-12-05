using LolStatsTracker.Services.LeagueAssetsService;
using LolStatsTracker.Services.StatsService;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Components;

namespace LolStatsTracker.Pages;

public partial class Analysis : IDisposable
{
    [Inject] private IStatsService StatsService { get; set; } = null!;
    [Inject] private ILeagueAssetsService LeagueAssetsService { get; set; } = null!;
    [Inject] private UserProfileState UserState { get; set; } = null!;

    private bool _isLoading = true;
    private List<DuoSummary> _duos = new();
    private List<DuoSummary> _worstDuos = new();
    private List<EnemyStatsDto> _enemyBotStats = new();
    private List<EnemyStatsDto> _enemySupportStats = new();

    protected override async Task OnInitializedAsync()
    {
        UserState.OnProfileChanged += OnProfileChangedAsync;
        
        await LeagueAssetsService.InitializeAsync();
        
        if (!UserState.IsInitialized)
            await UserState.InitializeAsync();
            
        await LoadDataAsync();
    }

    private async Task OnProfileChangedAsync()
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
            _isLoading = false;
            return;
        }

        var duosTask = StatsService.GetBestDuosAsync();
        var worstDuosTask = StatsService.GetWorstEnemyDuosAsync();
        var enemyBotTask = StatsService.GetEnemyStatsAsync("bot");
        var enemySupportTask = StatsService.GetEnemyStatsAsync("support");

        await Task.WhenAll(duosTask, worstDuosTask, enemyBotTask, enemySupportTask);

        _duos = await duosTask;
        _worstDuos = await worstDuosTask;

        _enemyBotStats = (await enemyBotTask).OrderByDescending(e => e.WinrateAgainst).ToList();
        _enemySupportStats = (await enemySupportTask).OrderByDescending(e => e.WinrateAgainst).ToList();

        _isLoading = false;
    }

    public void Dispose()
    {
        UserState.OnProfileChanged -= OnProfileChangedAsync;
    }
}
