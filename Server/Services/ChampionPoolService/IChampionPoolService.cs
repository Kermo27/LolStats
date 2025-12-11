using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.ChampionPoolService;

public interface IChampionPoolService
{
    Task<List<ChampionPoolDto>> GetPoolAsync(Guid profileId);
    Task<ChampionPoolDto?> GetByIdAsync(Guid id);
    Task<ChampionPool> CreateAsync(Guid profileId, ChampionPoolCreateDto dto);
    Task<ChampionPool?> UpdateAsync(Guid id, ChampionPoolUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
