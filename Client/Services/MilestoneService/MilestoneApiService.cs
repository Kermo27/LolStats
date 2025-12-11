using System.Net.Http.Json;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.MilestoneService;

public class MilestoneApiService : IMilestoneService
{
    private readonly HttpClient _http;
    private readonly UserProfileState _userState;

    public MilestoneApiService(HttpClient http, UserProfileState userState)
    {
        _http = http;
        _userState = userState;
    }

    public async Task<List<RankMilestoneDto>> GetMilestonesAsync()
    {
        if (_userState.CurrentProfile == null) return new List<RankMilestoneDto>();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Milestones");
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<RankMilestoneDto>>() ?? new();
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
