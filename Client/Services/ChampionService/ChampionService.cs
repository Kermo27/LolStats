using System.Net.Http.Json;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.ChampionService;

public class ChampionService : IChampionService
{
    private readonly HttpClient _http;
    private Dictionary<string, ChampionDto> _championsMap = new();
    private string _currentVersion = "14.23.1"; // Default backup
    private bool _initialized = false;

    public ChampionService(HttpClient http)
    {
        _http = http;
    }
    
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            var versions = await _http.GetFromJsonAsync<List<string>>("https://ddragon.leagueoflegends.com/api/versions.json");

            if (versions != null && versions.Any())
            {
                _currentVersion = versions.First();
            }
            
            var response = await _http.GetFromJsonAsync<DataDragonResponse>($"https://ddragon.leagueoflegends.com/cdn/{_currentVersion}/data/en_US/champion.json");

            if (response != null)
            {
                _championsMap =
                    response.Data.Values.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);
            }
            
            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd pobierania danych Riot: {ex.Message}");
        }
    }

    public IEnumerable<string> GetChampionNames()
    {
        return _championsMap.Keys.OrderBy(k => k);
    }

    public string GetChampionIconUrl(string championName)
    {
        if (string.IsNullOrWhiteSpace(championName)) return string.Empty;

        if (_championsMap.TryGetValue(championName, out var champion))
        {
            return $"https://ddragon.leagueoflegends.com/cdn/{_currentVersion}/img/champion/{champion.Id}.png";
        }

        return string.Empty;
    }
}