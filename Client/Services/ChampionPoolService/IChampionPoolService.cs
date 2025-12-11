using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.ChampionPoolService;

public interface IChampionPoolService
{
    Task<List<ChampionPoolDto>> GetPoolAsync();
    Task<ChampionPool> CreateAsync(ChampionPoolCreateDto dto);
    Task<ChampionPool?> UpdateAsync(Guid id, ChampionPoolUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
