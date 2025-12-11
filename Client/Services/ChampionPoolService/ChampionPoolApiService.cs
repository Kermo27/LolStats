using System.Net.Http.Json;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.ChampionPoolService;

public class ChampionPoolApiService : IChampionPoolService
{
    private readonly HttpClient _http;
    private readonly UserProfileState _userState;

    public ChampionPoolApiService(HttpClient http, UserProfileState userState)
    {
        _http = http;
        _userState = userState;
    }

    public async Task<List<ChampionPoolDto>> GetPoolAsync()
    {
        if (_userState.CurrentProfile == null) return new List<ChampionPoolDto>();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/ChampionPool");
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ChampionPoolDto>>() ?? new();
            }
            return new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }

    public async Task<ChampionPool> CreateAsync(ChampionPoolCreateDto dto)
    {
        if (_userState.CurrentProfile == null) 
            throw new InvalidOperationException("No profile selected");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/ChampionPool");
        AddProfileHeader(request);
        request.Content = JsonContent.Create(dto);
        
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChampionPool>() ?? new ChampionPool();
    }

    public async Task<ChampionPool?> UpdateAsync(Guid id, ChampionPoolUpdateDto dto)
    {
        if (_userState.CurrentProfile == null) return null;

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/ChampionPool/{id}");
        AddProfileHeader(request);
        request.Content = JsonContent.Create(dto);
        
        var response = await _http.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ChampionPool>();
        }
        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (_userState.CurrentProfile == null) return false;

        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/ChampionPool/{id}");
        AddProfileHeader(request);
        var response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
    
    private void AddProfileHeader(HttpRequestMessage request)
    {
        if (_userState.CurrentProfile != null)
        {
            request.Headers.Add("X-Profile-Id", _userState.CurrentProfile.Id.ToString());
        }
    }
}
