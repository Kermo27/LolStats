using System.Net.Http.Json;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.LeagueAssetsService;

public class LeagueAssetsService : ILeagueAssetsService
{
    private const string DDragonVersionUrl = "https://ddragon.leagueoflegends.com/api/versions.json";
    private const string DDragonBaseUrl = "https://ddragon.leagueoflegends.com/cdn";
    private const string CDragonRoleUrl = "https://raw.communitydragon.org/latest/plugins/rcp-fe-lol-static-assets/global/default/images/honor/roleicon_";
    
    private readonly HttpClient _http;
    private readonly ILogger<LeagueAssetsService> _logger;
    
    private Dictionary<string, ChampionDto> _championsMap = new();
    private List<string> _sortedChampionNames = new();
    private string _currentVersion = "14.23.1"; // Default backup
    
    public bool IsInitialized { get; private set; } = false;

    public LeagueAssetsService(HttpClient http, ILogger<LeagueAssetsService> logger)
    {
        _http = http;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        try
        {
            var version = await GetLatestVersionAsync();
            _currentVersion = version;
            
            var championUrl = $"https://ddragon.leagueoflegends.com/cdn/{_currentVersion}/data/en_US/champion.json";
            var response = await _http.GetFromJsonAsync<DataDragonResponse>(championUrl);

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
        var response = await _http.GetFromJsonAsync<List<string>>(DDragonVersionUrl);
        return response?.First() ?? _currentVersion;
    }
}