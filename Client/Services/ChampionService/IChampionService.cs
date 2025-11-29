namespace LolStatsTracker.Services.ChampionService;

public interface IChampionService
{
    Task InitializeAsync();
    IEnumerable<string> GetChampionNames();
    string GetChampionIconUrl(string championName);
}