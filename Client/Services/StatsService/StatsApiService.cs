using LolStatsTracker.Services.Common;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.StatsService;

public class StatsApiService : BaseApiService, IStatsService
{
    private const string BaseUrl = "api/Stats";

    public StatsApiService(HttpClient http, IUserProfileState userState) 
        : base(http, userState) { }
    
    public async Task<StatsSummaryDto?> GetSummaryAsync(int months = 6, int? seasonId = null, string? gameMode = null)
    {
        var url = BuildUrl($"{BaseUrl}/summary?months={months}", seasonId, gameMode);
        return await GetAsync<StatsSummaryDto>(url);
    }

    public async Task<List<ChampionStatsDto>> GetChampionsAsync(int? seasonId = null, string? gameMode = null)
    {
        var url = BuildUrl($"{BaseUrl}/champions", seasonId, gameMode);
        return await GetAsync<List<ChampionStatsDto>>(url) ?? new();
    }

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role, int? seasonId = null, string? gameMode = null)
    {
        var url = BuildUrl($"{BaseUrl}/enemies?role={role}", seasonId, gameMode);
        return await GetAsync<List<EnemyStatsDto>>(url) ?? new();
    }

    public async Task<List<DuoSummary>> GetBestDuosAsync(int? seasonId = null, string? gameMode = null)
    {
        var url = BuildUrl($"{BaseUrl}/best-duos", seasonId, gameMode);
        return await GetAsync<List<DuoSummary>>(url) ?? new();
    }

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync(int? seasonId = null, string? gameMode = null)
    {
        var url = BuildUrl($"{BaseUrl}/worst-enemy-duos", seasonId, gameMode);
        return await GetAsync<List<DuoSummary>>(url) ?? new();
    }
}