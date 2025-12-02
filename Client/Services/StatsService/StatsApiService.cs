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
    
    private async Task<T?> SendRequestAsync<T>(string url)
    {
        if (_userState.CurrentProfile == null)
        {
            Console.WriteLine($"[StatsService] No profile set, skipping request to {url}");
            return default;
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        request.Headers.Add("X-Profile-Id", _userState.CurrentProfile.Id.ToString());

        try
        {
            var response = await _http.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[StatsService] Błąd HTTP {response.StatusCode} dla {url}");
                return default;
            }
            
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StatsService] Error {url}: {ex.Message}");
            return default;
        }
    }
    
    public async Task<StatsSummaryDto?> GetSummaryAsync(int months = 6)
    {
        return await SendRequestAsync<StatsSummaryDto>($"api/Stats/summary?months={months}");
    }

    public async Task<List<ChampionStatsDto>> GetChampionsAsync()
    {
        return await SendRequestAsync<List<ChampionStatsDto>>("api/Stats/champions") ?? new();
    }

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync()
    {
        return await SendRequestAsync<List<DuoSummary>>("api/Stats/worst-enemy-duos") ?? new();
    }

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role)
    {
        return await SendRequestAsync<List<EnemyStatsDto>>($"api/Stats/enemies?role={role}") ?? new();
    }

    public async Task<List<DuoSummary>> GetBestDuosAsync()
    {
        return await SendRequestAsync<List<DuoSummary>>("api/Stats/best-duos") ?? new();
    }
}