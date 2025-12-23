using System.Net.Http.Json;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.LeagueAssetsService;

public class LeagueAssetsService : ILeagueAssetsService
{
    private const string CDragonRoleUrl = "https://raw.communitydragon.org/latest/plugins/rcp-fe-lol-static-assets/global/default/images/honor/roleicon_";
    // We still need this base URL for constructing image paths unless we proxy images too.
    // Given the task is about logic/data, we keep image loading from CDN for now but fetch data from Server.
    private const string DDragonBaseUrl = "https://ddragon.leagueoflegends.com/cdn"; 
    
    private readonly HttpClient _http;
    private readonly ILogger<LeagueAssetsService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    
    private Dictionary<string, ChampionDto> _championsMap = new();
    private List<string> _sortedChampionNames = new();
    private string _currentVersion = "14.23.1"; // Default backup
    
    public bool IsInitialized { get; private set; }

    public LeagueAssetsService(HttpClient http, ILogger<LeagueAssetsService> logger)
    {
        _http = http;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (IsInitialized) return; // Double-check after acquiring lock
            
            // 1. Get Version from Server
            var version = await GetLatestVersionAsync();
            _currentVersion = version;
            
            // 2. Get Champions from Server
            var response = await _http.GetFromJsonAsync<DataDragonResponse>("/api/assets/champions");

            if (response?.Data != null)
            {
                _championsMap = response.Data.Values
                    .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

                _sortedChampionNames = _championsMap.Keys
                    .OrderBy(k => k)
                    .ToList();
            }
        
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to initialize LeagueAssetsService: {ex.Message}");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public IEnumerable<string> GetChampionNames()
    {
        return _sortedChampionNames;
    }

    public string GetChampionIconUrl(string championName)
    {
        if (string.IsNullOrWhiteSpace(championName)) return string.Empty;
        
        if (_championsMap.TryGetValue(championName, out var champion))
        {
            return $"{DDragonBaseUrl}/{_currentVersion}/img/champion/{champion.Id}.png";
        }
        
        return IsInitialized 
            ? string.Empty
            : $"{DDragonBaseUrl}/{_currentVersion}/img/champion/{championName}.png"; 
    }

    public string GetRoleIconUrl(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return string.Empty;
        
        var fileSuffix = role.ToLower() switch
        {
            "top" => "top.png",
            "jungle" => "jungle.png",
            "mid" => "middle.png",
            "adc" => "bottom.png",
            "support" => "utility.png",
            _ => null
        };

        return fileSuffix != null ? $"{CDragonRoleUrl}{fileSuffix}" : string.Empty;
    }
    
    private async Task<string> GetLatestVersionAsync()
    {
        try
        {
             // Call Server API instead of DDragon directly
             // The API returns the version string directly (e.g. "14.23.1")
             // Note: Controller returns ActionResult<string>, so checks are needed or just ReadAsString
             // But GetFromJsonAsync<string> expects a JSON JSON string "14.23.1" which Ok("val") produces.
             var version = await _http.GetStringAsync("/api/assets/version");
             return version.Trim('"'); // Remove quotes if JSON serialization added them
        }
        catch 
        {
            return _currentVersion;
        }
    }
}
