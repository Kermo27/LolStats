using System.Net.Http.Json;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.StatsService;

public class StatsApiService : IStatsService
{
    private readonly HttpClient _http;
    private readonly UserProfileState _userState;

    public StatsApiService(HttpClient http, UserProfileState userState)
    {
        _http = http;
        _userState = userState;
    }
    
    private string BuildUrl(string baseUrl, int? seasonId, string? gameMode = null)
    {
        var url = baseUrl;
        var hasQuery = baseUrl.Contains('?');
        
        if (seasonId.HasValue)
        {
            url += (hasQuery ? "&" : "?") + $"seasonId={seasonId}";
            hasQuery = true;
        }
        
        if (!string.IsNullOrEmpty(gameMode) && gameMode != "All")
        {
            url += (hasQuery ? "&" : "?") + $"gameMode={Uri.EscapeDataString(gameMode)}";
        }
        
        return url;
    }
    
    public async Task<StatsSummaryDto?> GetSummaryAsync(int months = 6, int? seasonId = null, string? gameMode = null)
    {
        if (_userState.CurrentProfile == null) return null;
        
        try
        {
            var url = BuildUrl($"api/Stats/summary?months={months}", seasonId, gameMode);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<StatsSummaryDto>();
            }
            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[StatsService] Error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ChampionStatsDto>> GetChampionsAsync(int? seasonId = null, string? gameMode = null)
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var url = BuildUrl("api/Stats/champions", seasonId, gameMode);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ChampionStatsDto>>() ?? new();
            }
            return new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync(int? seasonId = null, string? gameMode = null)
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var url = BuildUrl("api/Stats/worst-enemy-duos", seasonId, gameMode);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DuoSummary>>() ?? new();
            }
            return new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role, int? seasonId = null, string? gameMode = null)
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var url = BuildUrl($"api/Stats/enemies?role={role}", seasonId, gameMode);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<EnemyStatsDto>>() ?? new();
            }
            return new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }

    public async Task<List<DuoSummary>> GetBestDuosAsync(int? seasonId = null, string? gameMode = null)
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var url = BuildUrl("api/Stats/best-duos", seasonId, gameMode);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DuoSummary>>() ?? new();
            }
            return new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }
    
    private void AddProfileHeader(HttpRequestMessage request)
    {
        if (_userState.CurrentProfile != null)
        {
            request.Headers.Add("X-Profile-Id", _userState.CurrentProfile.Id.ToString());
        }
    }
}