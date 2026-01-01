using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.SeasonState;

public interface ISeasonState
{
    event Action? OnChange;
    event Func<Task>? OnSeasonChanged;
    
    Season? CurrentSeason { get; }
    List<Season> AllSeasons { get; }
    bool IsInitialized { get; }
    bool IsFilteringEnabled { get; }
    
    Task InitializeAsync();
    Task SetActiveSeasonAsync(Season? season);
    Task ClearSeasonFilterAsync();
    (DateTime? StartDate, DateTime? EndDate) GetCurrentSeasonDates();
    bool IsDateInCurrentSeason(DateTime date);
}
