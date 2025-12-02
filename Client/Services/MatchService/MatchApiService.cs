using System.Net.Http.Json;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.MatchService;

public class MatchApiService : IMatchService
{
    private readonly HttpClient _http;
    private readonly UserProfileState _userState;

    public MatchApiService(HttpClient http, UserProfileState userState)
    {
        _http = http;
        _userState = userState;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, object? content = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (_userState.CurrentProfile != null)
        {
            request.Headers.Add("X-Profile-Id", _userState.CurrentProfile.Id.ToString());
        }
        else
        {
            Console.WriteLine("[MatchService] No profile set, skipping header.");
        }

        if (content != null)
        {
            request.Content = JsonContent.Create(content);
        }

        return request;
    }
    
    public async Task<List<MatchEntry>> GetAllAsync()
    {
        if (_userState.CurrentProfile == null) return new List<MatchEntry>();

        var request = CreateRequest(HttpMethod.Get, "api/Matches");
        var response = await _http.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<MatchEntry>>() ?? new();
        }
        return new();
    }
    
    public async Task<MatchEntry?> GetAsync(Guid id)
    {
        if (_userState.CurrentProfile == null) return null;

        var request = CreateRequest(HttpMethod.Get, $"api/Matches/{id}");
        var response = await _http.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<MatchEntry>();
        }
        return null;
    }

    public async Task<MatchEntry> AddAsync(MatchEntry match)
    {
        if (_userState.CurrentProfile == null) throw new InvalidOperationException("Brak profilu");

        var request = CreateRequest(HttpMethod.Post, "api/Matches", match);
        var response = await _http.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchEntry>() ?? match;
    }

    public async Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match)
    {
        if (_userState.CurrentProfile == null) throw new InvalidOperationException("Brak profilu");

        var request = CreateRequest(HttpMethod.Put, $"api/Matches/{id}", match);
        var response = await _http.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchEntry>() ?? match;
    }

    public async Task DeleteAsync(Guid id)
    {
        if (_userState.CurrentProfile == null) return;

        var request = CreateRequest(HttpMethod.Delete, $"api/Matches/{id}");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task ClearAsync()
    {
        if (_userState.CurrentProfile == null) return;

        var request = CreateRequest(HttpMethod.Delete, "api/Matches/clear");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}