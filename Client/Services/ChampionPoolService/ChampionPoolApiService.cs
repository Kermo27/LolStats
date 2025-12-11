using LolStatsTracker.Services.Common;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.ChampionPoolService;

public class ChampionPoolApiService : BaseApiService, IChampionPoolService
{
    private const string BaseUrl = "api/ChampionPool";

    public ChampionPoolApiService(HttpClient http, UserProfileState userState) 
        : base(http, userState) { }

    public async Task<List<ChampionPoolDto>> GetPoolAsync()
    {
        return await GetAsync<List<ChampionPoolDto>>(BaseUrl) ?? new List<ChampionPoolDto>();
    }

    public async Task<ChampionPool> CreateAsync(ChampionPoolCreateDto dto)
    {
        return await PostAsync<ChampionPoolCreateDto, ChampionPool>(BaseUrl, dto) ?? new ChampionPool();
    }

    public async Task<ChampionPool?> UpdateAsync(Guid id, ChampionPoolUpdateDto dto)
    {
        return await PutAsync<ChampionPoolUpdateDto, ChampionPool>($"{BaseUrl}/{id}", dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await DeleteAsync($"{BaseUrl}/{id}");
    }
}
