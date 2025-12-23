using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.API.Services.DDragonService;

public interface IDDragonService
{
    Task<string> GetLatestVersionAsync();
    Task<DataDragonResponse?> GetChampionsAsync();
}
