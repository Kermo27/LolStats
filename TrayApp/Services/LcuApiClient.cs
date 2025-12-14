using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Models.Lcu;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LolStatsTracker.TrayApp.Services;

public class LcuApiClient : IDisposable
{
    private readonly ILogger<LcuApiClient> _logger;
    private HttpClient? _httpClient;
    private LcuConnectionInfo? _connectionInfo;
    
    public LcuApiClient(ILogger<LcuApiClient> logger)
    {
        _logger = logger;
    }
    
    public void Initialize(LcuConnectionInfo connectionInfo)
    {
        _connectionInfo = connectionInfo;
        
        // Create HttpClient with SSL bypass
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(connectionInfo.BaseUrl)
        };
        
        // Add Basic Auth header
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", connectionInfo.GetBasicAuthToken());
        
        _logger.LogInformation("LCU API Client initialized for {BaseUrl}", connectionInfo.BaseUrl);
    }
    
    public async Task<LcuSummoner?> GetCurrentSummonerAsync()
    {
        try
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Client not initialized");
            
            var response = await _httpClient.GetAsync("/lol-summoner/v1/current-summoner");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get current summoner: {StatusCode}", response.StatusCode);
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LcuSummoner>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current summoner");
            return null;
        }
    }
    
    public async Task<LcuQueueStats?> GetRankedStatsAsync()
    {
        try
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Client not initialized");
            
            var response = await _httpClient.GetAsync("/lol-ranked/v1/current-ranked-stats");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get ranked stats: {StatusCode}", response.StatusCode);
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var rankedStats = JsonConvert.DeserializeObject<LcuRankedStats>(json);
            
            // Get RANKED_SOLO_5x5 queue
            return rankedStats?.Queues.FirstOrDefault(q => q.QueueType == "RANKED_SOLO_5x5");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ranked stats");
            return null;
        }
    }
    
    public async Task<LcuEndOfGameStats?> GetEndOfGameStatsAsync()
    {
        try
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Client not initialized");
            
            var response = await _httpClient.GetAsync("/lol-end-of-game/v1/eog-stats-block");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("End of game stats not available: {StatusCode}", response.StatusCode);
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LcuEndOfGameStats>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting end of game stats");
            return null;
        }
    }
    
    public async Task<LcuGame?> GetGameDetailsAsync(long gameId)
    {
        try
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Client not initialized");
            
            var response = await _httpClient.GetAsync($"/lol-match-history/v1/games/{gameId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Game details not available for {GameId}: {StatusCode}", gameId, response.StatusCode);
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LcuGame>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game details for {GameId}", gameId);
            return null;
        }
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
