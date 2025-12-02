namespace LolStatsTracker.Services.LeagueAssetsService;

public interface ILeagueAssetsService
{
    Task InitializeAsync();
    IEnumerable<string> GetChampionNames();
    string GetChampionIconUrl(string championName);
    string GetRoleIconUrl(string role);
    bool IsInitialized { get; }
}