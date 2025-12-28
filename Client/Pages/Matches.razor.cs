using System.Text.Json;
using LolStatsTracker.Components;
using LolStatsTracker.Services.MatchService;
using LolStatsTracker.Services.SeasonState;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace LolStatsTracker.Pages;

public partial class Matches : IDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IMatchService MatchService { get; set; } = null!;
    [Inject] private UserProfileState UserState { get; set; } = null!;
    [Inject] private SeasonState SeasonState { get; set; } = null!;

    private MatchEntry currentMatch = new();
    private List<MatchEntry> _allMatches = new();
    private List<MatchEntry> matches = new();
    private bool isEditing;

    private static readonly string[] Tiers =
    {
        "Iron", "Bronze", "Silver", "Gold", "Platinum", "Emerald", "Diamond", "Master", "Grandmaster", "Challenger"
    };

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to profile and season changes
        UserState.OnProfileChanged += OnProfileChangedAsync;
        SeasonState.OnSeasonChanged += OnSeasonChangedAsync;
        
        if (!UserState.IsInitialized)
        {
            await UserState.InitializeAsync();
        }
        
        if (!SeasonState.IsInitialized)
        {
            await SeasonState.InitializeAsync();
        }
        
        if (UserState.CurrentProfile == null)
        {
            Console.WriteLine("No profile.");
            return;
        }
        
        await LoadDataAsync();
        PrepareNewMatch();
    }

    private async Task OnProfileChangedAsync()
    {
        await LoadDataAsync();
        PrepareNewMatch();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnSeasonChangedAsync()
    {
        ApplySeasonFilter();
        await InvokeAsync(StateHasChanged);
    }

    private void ApplySeasonFilter()
    {
        matches = _allMatches
            .Where(m => SeasonState.IsDateInCurrentSeason(m.Date))
            .OrderByDescending(m => m.Date)
            .ToList();
    }

    private void PrepareNewMatch()
    {
        isEditing = false;
        currentMatch = new MatchEntry();
        
        if (!matches.Any())
            return;

        var lastMatch = matches.First();
        currentMatch.Role = lastMatch.Role;

        // Pre-fill with last match's rank (user will update LP after the new game)
        currentMatch.CurrentTier = lastMatch.CurrentTier;
        currentMatch.CurrentDivision = lastMatch.CurrentDivision;
        currentMatch.CurrentLp = lastMatch.CurrentLp;
        currentMatch.Date = DateTime.Now;
    }

    private async Task LoadDataAsync()
    {
        _allMatches = (await MatchService.GetAllAsync())
            .OrderByDescending(m => m.Date)
            .ToList();
        
        ApplySeasonFilter();
    }

    private async Task SaveMatchData(MatchEntry matchData)
    {
        if (isEditing)
        {
            await MatchService.UpdateAsync(matchData.Id, matchData);
            Snackbar.Add("Match updated", Severity.Success);
        }
        else
        {
            await MatchService.AddAsync(matchData);
            Snackbar.Add("Match added", Severity.Success);
        }
        
        await LoadDataAsync();
        PrepareNewMatch();
    }
    
    private void StartEdit(MatchEntry match)
    {
        currentMatch = new MatchEntry
        {
            Id = match.Id,
            ProfileId = match.ProfileId,
            Champion = match.Champion,
            Role = match.Role,
            LaneAlly = match.LaneAlly,
            LaneEnemy = match.LaneEnemy,
            LaneEnemyAlly = match.LaneEnemyAlly,
            Kills = match.Kills,
            Deaths = match.Deaths,
            Assists = match.Assists,
            Cs = match.Cs,
            GameLengthMinutes = match.GameLengthMinutes,
            Date = match.Date,
            Win = match.Win,
            CurrentTier = match.CurrentTier,
            CurrentDivision = match.CurrentDivision,
            CurrentLp = match.CurrentLp
        };
        isEditing = true;
    }

    private void ResetForm() => PrepareNewMatch();

    private async Task DeleteMatch(MatchEntry match)
    {
        await MatchService.DeleteAsync(match.Id);
        matches.Remove(match);
        Snackbar.Add("Match deleted", Severity.Success);
        
        if (!isEditing)
            PrepareNewMatch();
    }
    
    private async Task ExportJson()
    {
        try
        {
            var allMatches = await MatchService.GetAllAsync();
            var json = JsonSerializer.Serialize(allMatches, new JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);

            await JS.InvokeVoidAsync("downloadFile", "matches.json", "application/json", base64);
            Snackbar.Add("Exported", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Export error: {ex.Message}", Severity.Error);
        }
    }

    private async Task ImportJson(InputFileChangeEventArgs e)
    {
        try 
        {
            await using var stream = e.File.OpenReadStream(5 * 1024 * 1024); 
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var imported = JsonSerializer.Deserialize<List<MatchEntry>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (imported is null || !imported.Any()) return;
            
            var addedCount = 0;
            foreach (var match in imported)
            {
                if (match.Id == Guid.Empty) match.Id = Guid.NewGuid();
                
                if (matches.All(existing => existing.Id != match.Id))
                {
                    await MatchService.AddAsync(match);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await LoadDataAsync();
                Snackbar.Add($"Imported {addedCount} matches", Severity.Success);
            }
            else
            {
                Snackbar.Add("No new matches to import", Severity.Info);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Import error: {ex.Message}", Severity.Error);
        }
    }

    private async Task ClearData()
    {
        var parameters = new DialogParameters
        {
            { "ContentText", "Are you sure you want to delete ALL match history? This operation cannot be undone!" },
            { "ButtonText", "Delete all" },
            { "Color", Color.Error }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };

        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Danger Zone", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true })
        {
            await MatchService.ClearAsync();
            matches.Clear();
            PrepareNewMatch();
            Snackbar.Add("Database was erased", Severity.Warning);
        }
    }

    public void Dispose()
    {
        UserState.OnProfileChanged -= OnProfileChangedAsync;
        SeasonState.OnSeasonChanged -= OnSeasonChangedAsync;
    }
}
