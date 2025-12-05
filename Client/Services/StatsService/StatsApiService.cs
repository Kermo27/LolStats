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
    
    public async Task<StatsSummaryDto?> GetSummaryAsync(int months = 6)
    {
        if (_userState.CurrentProfile == null) return null;
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Stats/summary?months={months}");
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

    public async Task<List<ChampionStatsDto>> GetChampionsAsync()
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Stats/champions");
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

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync()
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Stats/worst-enemy-duos");
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

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role)
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Stats/enemies?role={role}");
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

    public async Task<List<DuoSummary>> GetBestDuosAsync()
    {
        if (_userState.CurrentProfile == null) return new();
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Stats/best-duos");
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