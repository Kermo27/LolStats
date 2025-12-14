using System.Net.Http;
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
    
    public ApiSyncService(
        ILogger<ApiSyncService> logger,
        IOptions<AppConfiguration> config,
        HttpClient httpClient)
    {
        _logger = logger;
        _config = config.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_config.ApiBaseUrl);
    }
    
    public async Task<bool> SyncMatchAsync(MatchEntry match)
    {
        try
        {
            if (_config.ProfileId == Guid.Empty)
            {
                _logger.LogWarning("Profile ID not configured, cannot sync match");
                return false;
            }
            
            match.ProfileId = _config.ProfileId;
            
            var json = JsonConvert.SerializeObject(match);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Add Profile ID header
            if (!_httpClient.DefaultRequestHeaders.Contains("X-Profile-Id"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Profile-Id", _config.ProfileId.ToString());
            }
            
            _logger.LogInformation("Syncing match: {Champion} {Result}", match.Champion, match.Win ? "Win" : "Loss");
            
            var response = await _httpClient.PostAsync("/api/matches", content);
            
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
            var response = await _httpClient.GetAsync("/api/matches");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to API");
            return false;
        }
    }
}
