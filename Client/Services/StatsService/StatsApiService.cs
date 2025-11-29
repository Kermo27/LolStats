using System.Net.Http.Json;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.StatsService;

public class StatsApiService : IStatsService
{
    private readonly HttpClient _http;

    public StatsApiService(HttpClient http)
    {
        _http = http;
    }
    
    public async Task<StatsSummaryDto?> GetSummaryAsync(int months = 6)
    {
        try
        {
            return await _http.GetFromJsonAsync<StatsSummaryDto>($"http://localhost:5031/api/stats/summary?months={months}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd pobierania statystyk: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ChampionStatsDto>> GetChampionsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<ChampionStatsDto>>($"http://localhost:5031/api/stats/champions");
            
            return result ?? new List<ChampionStatsDto>();
        }
        catch (Exception ex)
        {
            Console.Write($"Error fetching champions: {ex.Message}");
            return new List<ChampionStatsDto>();
        }
    }

    public async Task<List<DuoSummary>> GetWorstEnemyDuosAsync()
    {
        try 
        {
            var result = await _http.GetFromJsonAsync<List<DuoSummary>>("http://localhost:5031/api/stats/worst-enemy-duos");
            return result ?? new List<DuoSummary>();
        }
        catch { return new List<DuoSummary>(); }
    }

    public async Task<List<EnemyStatsDto>> GetEnemyStatsAsync(string role)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<EnemyStatsDto>>($"http://localhost:5031/api/stats/enemies?role={role}");
            return result ?? new List<EnemyStatsDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching enemies: {ex.Message}");
            return new List<EnemyStatsDto>();
        }
    }

    public async Task<List<DuoSummary>> GetBestDuosAsync()
    {
        try 
        {
            var result = await _http.GetFromJsonAsync<List<DuoSummary>>("http://localhost:5031/api/stats/best-duos");
            return result ?? new List<DuoSummary>();
        }
        catch { return new List<DuoSummary>(); }
    }
}