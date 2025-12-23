using LolStatsTracker.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace LolStatsTracker.API.Services.DDragonService;

public class DDragonService : IDDragonService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DDragonService> _logger;

    private const string DDragonVersionUrl = "https://ddragon.leagueoflegends.com/api/versions.json";
    private const string CacheKeyVersion = "ddragon_version";
    private const string CacheKeyChampions = "ddragon_champions";

    public DDragonService(HttpClient httpClient, IMemoryCache cache, ILogger<DDragonService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetLatestVersionAsync()
    {
        if (_cache.TryGetValue(CacheKeyVersion, out string? cachedVersion) && !string.IsNullOrEmpty(cachedVersion))
        {
            return cachedVersion;
        }

        try
        {
            var versions = await _httpClient.GetFromJsonAsync<List<string>>(DDragonVersionUrl);
            var latest = versions?.FirstOrDefault() ?? "14.23.1";
            
            _cache.Set(CacheKeyVersion, latest, TimeSpan.FromHours(1));
            return latest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch DDragon version");
            return "14.23.1"; // Fallback
        }
    }

    public async Task<DataDragonResponse?> GetChampionsAsync()
    {
        if (_cache.TryGetValue(CacheKeyChampions, out DataDragonResponse? cachedData))
        {
            return cachedData;
        }

        try
        {
            var version = await GetLatestVersionAsync();
            var url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json";
            
            var response = await _httpClient.GetFromJsonAsync<DataDragonResponse>(url);
            
            if (response != null)
            {
                _cache.Set(CacheKeyChampions, response, TimeSpan.FromHours(1));
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch DDragon champions");
            return null;
        }
    }
}
