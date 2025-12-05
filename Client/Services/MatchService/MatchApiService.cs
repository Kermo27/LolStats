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

    public async Task<List<MatchEntry>> GetAllAsync()
    {
        if (_userState.CurrentProfile == null) return new List<MatchEntry>();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Matches");
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<MatchEntry>>() ?? new();
            }
            return new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }
    
    public async Task<MatchEntry?> GetAsync(Guid id)
    {
        if (_userState.CurrentProfile == null) return null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Matches/{id}");
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MatchEntry>();
            }
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<MatchEntry> AddAsync(MatchEntry match)
    {
        if (_userState.CurrentProfile == null) 
            throw new InvalidOperationException("No profile selected");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/Matches");
        AddProfileHeader(request);
        request.Content = JsonContent.Create(match);
        
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchEntry>() ?? match;
    }

    public async Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match)
    {
        if (_userState.CurrentProfile == null) 
            throw new InvalidOperationException("No profile selected");

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/Matches/{id}");
        AddProfileHeader(request);
        request.Content = JsonContent.Create(match);
        
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchEntry>() ?? match;
    }

    public async Task DeleteAsync(Guid id)
    {
        if (_userState.CurrentProfile == null) return;

        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Matches/{id}");
        AddProfileHeader(request);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task ClearAsync()
    {
        if (_userState.CurrentProfile == null) return;

        var request = new HttpRequestMessage(HttpMethod.Delete, "api/Matches/clear");
        AddProfileHeader(request);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
    
    private void AddProfileHeader(HttpRequestMessage request)
    {
        if (_userState.CurrentProfile != null)
        {
            request.Headers.Add("X-Profile-Id", _userState.CurrentProfile.Id.ToString());
        }
    }
}