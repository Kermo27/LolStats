using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LolStatsTracker.TrayApp.Services;

public class ChampionDataService
{
    private readonly HttpClient _httpClient;
    private Dictionary<int, string> _championIdToName = new();
    private bool _initialized;
    
    public ChampionDataService()
    {
        _httpClient = new HttpClient();
    }
    
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        
        try
        {
            // Get latest version
            var versions = await _httpClient.GetFromJsonAsync<List<string>>(
                "https://ddragon.leagueoflegends.com/api/versions.json");
            var version = versions?.FirstOrDefault() ?? "14.24.1";
            
            // Get champions data
            var url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json";
            var response = await _httpClient.GetFromJsonAsync<DDragonChampionResponse>(url);
            
            if (response?.Data != null)
            {
                _championIdToName = response.Data.Values
                    .Where(c => int.TryParse(c.Key, out _))
                    .ToDictionary(
                        c => int.Parse(c.Key),
                        c => c.Name
                    );
                
                Console.WriteLine($"[ChampionDataService] Loaded {_championIdToName.Count} champions from DDragon v{version}");
            }
            
            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChampionDataService] Failed to load champions: {ex.Message}");
            // Fallback - will return "Champion_ID" format
        }
    }
    
    public string GetChampionName(int championId)
    {
        return _championIdToName.TryGetValue(championId, out var name) 
            ? name 
            : $"Champion_{championId}";
    }
    
    public bool IsInitialized => _initialized;
}

// DDragon response models
public class DDragonChampionResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, DDragonChampion> Data { get; set; } = new();
}

public class DDragonChampion
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = ""; // Numeric ID as string (e.g. "51")
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = ""; // Display name (e.g. "Caitlyn")
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = ""; // Internal ID (e.g. "Caitlyn")
}
