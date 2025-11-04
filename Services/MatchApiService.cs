using System.Net.Http.Json;
using LolStatsTracker.Models;

namespace LolStatsTracker.Services;

public class MatchApiService : IMatchService
{
    private readonly HttpClient _http;

    public MatchApiService(HttpClient http)
    {
        _http = http;
    }

    private const string BaseUrl = "http://localhost:5031/matches";

    public async Task<List<MatchEntry>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<MatchEntry>>(BaseUrl) ?? new List<MatchEntry>();
    }
    
    public async Task<MatchEntry?> GetAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<MatchEntry>($"{BaseUrl}/{id}");
    }

    public async Task<MatchEntry> AddAsync(MatchEntry match)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, match);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchEntry>()!;
    }

    public async Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", match);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchEntry>()!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ClearAsync()
    {
        var response = await _http.DeleteAsync(BaseUrl);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<MatchStats>> GetChampionStatsAsync()
    {
        var matches = await GetAllAsync();
        return matches
            .GroupBy(m => m.Champion)
            .Select(g => new MatchStats
            {
                Champion = g.Key,
                Games = g.Count(),
                Wins = g.Count(m => m.Win),
                WinRate = Math.Round(100.0 * g.Count(m => m.Win) / g.Count(), 1),
                AvgKda = Math.Round(g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths)), 2),
                AvgCsm = Math.Round(g.Average(m => m.Cs / Math.Max(1.0, m.GameLengthMinutes)), 2)
            })
            .OrderByDescending(s => s.Games);
    }

    public async Task<IEnumerable<SupportStats>> GetSupportStatsAsync()
    {
        var matches = await GetAllAsync();
        return matches
            .Where(m => !string.IsNullOrWhiteSpace(m.Support))
            .GroupBy(m => m.Support)
            .Select(g => new SupportStats
            {
                Support = g.Key,
                Games = g.Count(),
                Wins = g.Count(m => m.Win),
                WinRate = Math.Round(100.0 * g.Count(m => m.Win) / g.Count(), 1),
                AvgKda = Math.Round(g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths)), 2)
            })
            .OrderByDescending(s => s.Games)
            .ThenByDescending(s => s.WinRate);
    }

    public async Task<IEnumerable<EnemyStats>> GetEnemyBotStatsAsync()
    {
        var matches = await GetAllAsync();
        return matches
            .Where(m => !string.IsNullOrWhiteSpace(m.EnemyBot))
            .GroupBy(m => m.EnemyBot)
            .Select(g => new EnemyStats
            {
                Enemy = g.Key,
                Games = g.Count(),
                Wins = g.Count(m => m.Win),
                WinRate = Math.Round(100.0 * g.Count(m => m.Win) / g.Count(), 1)
            })
            .OrderByDescending(s => s.Games)
            .ThenByDescending(s => s.WinRate);
    }

    public async Task<IEnumerable<EnemySupportStats>> GetEnemySupportStatsAsync()
    {
        var matches = await GetAllAsync();
        return matches
            .Where(m => !string.IsNullOrWhiteSpace(m.EnemySupport))
            .GroupBy(m => m.EnemySupport)
            .Select(g => new EnemySupportStats
            {
                Enemy = g.Key,
                Games = g.Count(),
                Wins = g.Count(m => m.Win),
                WinRate = Math.Round(100.0 * g.Count(m => m.Win) / g.Count(), 1)
            })
            .OrderByDescending(s => s.Games)
            .ThenByDescending(s => s.WinRate);
    }
}