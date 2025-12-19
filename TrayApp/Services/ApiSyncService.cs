using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using LolStatsTracker.Shared.Models;
using LolStatsTracker.TrayApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LolStatsTracker.TrayApp.Services;

public class ApiSyncService
{
    private readonly ILogger<ApiSyncService> _logger;
    private readonly HttpClient _httpClient;
    private readonly AppConfiguration _config;
    private readonly TrayAuthService _authService;
    
    public ApiSyncService(
        ILogger<ApiSyncService> logger,
        IOptions<AppConfiguration> config,
        HttpClient httpClient,
        TrayAuthService authService)
    {
        _logger = logger;
        _config = config.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_config.ApiBaseUrl);
        _authService = authService;
    }
    
    public async Task<bool> SyncMatchAsync(MatchEntry match)
    {
        try
        {
            var token = await _authService.GetValidAccessTokenAsync();
            if (token == null)
            {
                _logger.LogWarning("Not authenticated (or token refresh failed), cannot sync match");
                return false;
            }
            
            if (match.ProfileId == Guid.Empty)
            {
                _logger.LogWarning("Match has no ProfileId, cannot sync");
                return false;
            }
            
            var json = JsonConvert.SerializeObject(match);
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/matches");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Add Profile ID header
            request.Headers.Add("X-Profile-Id", match.ProfileId.ToString());
            
            // Add JWT Authorization header
            _authService.AddAuthHeader(request);
            
            _logger.LogInformation("Syncing match: {Champion} {Result}", match.Champion, match.Win ? "Win" : "Loss");
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Match synced successfully: {Champion} ({KDA})", 
                    match.Champion, match.KdaDisplay);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to sync match: {StatusCode} - {Error}", 
                    response.StatusCode, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing match to API");
            return false;
        }
    }
    
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            _authService.AddAuthHeader(request);
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to API");
            return false;
        }
    }
    
    public async Task<Guid?> GetOrCreateProfileAsync(string summonerName, string tag, string? puuid = null)
    {
        try
        {
            var token = await _authService.GetValidAccessTokenAsync();
            if (token == null)
            {
                _logger.LogWarning("Not authenticated (or token refresh failed), cannot get/create profile");
                return null;
            }
            
            // Get existing profiles
            var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/profiles");
            _authService.AddAuthHeader(getRequest);
            
            var getResponse = await _httpClient.SendAsync(getRequest);
            if (!getResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get profiles: {StatusCode}", getResponse.StatusCode);
                return null;
            }
            
            var profiles = await getResponse.Content.ReadFromJsonAsync<List<UserProfile>>();
            
            // Check if profile with this PUUID already exists
            if (!string.IsNullOrEmpty(puuid) && profiles != null)
            {
                var existingProfile = profiles.FirstOrDefault(p => p.RiotPuuid == puuid);
                if (existingProfile != null)
                {
                    _logger.LogInformation("Found existing profile for PUUID: {Id}", existingProfile.Id);
                    return existingProfile.Id;
                }
            }
            
            // Check if profile with matching name exists
            if (profiles != null)
            {
                var matchingProfile = profiles.FirstOrDefault(p => 
                    p.Name.Equals(summonerName, StringComparison.OrdinalIgnoreCase) &&
                    p.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase));
                if (matchingProfile != null)
                {
                    _logger.LogInformation("Found existing profile by name: {Id}", matchingProfile.Id);
                    return matchingProfile.Id;
                }
            }
            
            // Create new profile
            var newProfile = new UserProfile
            {
                Name = summonerName,
                Tag = tag,
                RiotPuuid = puuid,
                IsDefault = profiles == null || !profiles.Any()
            };
            
            var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/profiles");
            createRequest.Content = new StringContent(
                JsonConvert.SerializeObject(newProfile),
                Encoding.UTF8,
                "application/json"
            );
            _authService.AddAuthHeader(createRequest);
            
            var createResponse = await _httpClient.SendAsync(createRequest);
            if (createResponse.IsSuccessStatusCode)
            {
                var created = await createResponse.Content.ReadFromJsonAsync<UserProfile>();
                if (created != null)
                {
                    _logger.LogInformation("Created new profile: {Id} ({Name}#{Tag})", 
                        created.Id, created.Name, created.Tag);
                    return created.Id;
                }
            }
            
            _logger.LogWarning("Failed to create profile: {StatusCode}", createResponse.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting/creating profile");
            return null;
        }
    }
}
