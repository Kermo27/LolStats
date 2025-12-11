using System.Net.Http.Json;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.MetaService;

public class MetaApiService : IMetaService
{
    private readonly HttpClient _http;
    private readonly UserProfileState _userState;

    public MetaApiService(HttpClient http, UserProfileState userState)
    {
        _http = http;
        _userState = userState;
    }

    public async Task<MetaComparisonSummaryDto?> GetComparisonAsync()
    {
        if (_userState.CurrentProfile == null) return null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Meta/comparison");
            AddProfileHeader(request);
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MetaComparisonSummaryDto>();
            }
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
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
