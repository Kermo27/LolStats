using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LolStatsTracker.TrayApp.Services;

/// <summary>
/// Service for interacting with Riot Games official API
/// </summary>
public class RiotApiService
{
    private readonly ILogger<RiotApiService> _logger;
    private readonly IUserSettingsService _settingsService;
    private readonly HttpClient _httpClient;
    
    // Regional routing for different API endpoints
    private static readonly Dictionary<string, string> RegionalRouting = new()
    {
        // Platform routing (for summoner, match-v5 by match ID)
        { "euw1", "europe" },
        { "eun1", "europe" },
        { "tr1", "europe" },
        { "ru", "europe" },
        { "na1", "americas" },
        { "br1", "americas" },
        { "la1", "americas" },
        { "la2", "americas" },
        { "kr", "asia" },
        { "jp1", "asia" },
        { "oc1", "sea" },
        { "ph2", "sea" },
        { "sg2", "sea" },
        { "th2", "sea" },
        { "tw2", "sea" },
        { "vn2", "sea" }
    };
    
    public bool IsConfigured => !string.IsNullOrEmpty(_settingsService.Settings.RiotApiKey);
    
    public RiotApiService(
        ILogger<RiotApiService> logger,
        IUserSettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _httpClient = new HttpClient();
    }
    
    private string? ApiKey => _settingsService.Settings.RiotApiKey;
    private string Region => _settingsService.Settings.RiotRegion;
    private string RegionalEndpoint => RegionalRouting.GetValueOrDefault(Region, "europe");
    
    /// <summary>
    /// Get summoner by PUUID
    /// </summary>
    public async Task<RiotSummoner?> GetSummonerByPuuidAsync(string puuid)
    {
        if (!IsConfigured) return null;
        
        try
        {
            var url = $"https://{Region}.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}";
            var response = await SendRequestAsync<RiotSummoner>(url);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get summoner by PUUID");
            return null;
        }
    }
    
    /// <summary>
    /// Get account by Riot ID (gameName#tagLine)
    /// </summary>
    public async Task<RiotAccount?> GetAccountByRiotIdAsync(string gameName, string tagLine)
    {
        if (!IsConfigured) return null;
        
        try
        {
            var url = $"https://{RegionalEndpoint}.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}";
            var response = await SendRequestAsync<RiotAccount>(url);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get account by Riot ID");
            return null;
        }
    }
    
    /// <summary>
    /// Get match IDs for a player (most recent matches)
    /// </summary>
    public async Task<List<string>?> GetMatchIdsAsync(string puuid, int count = 20, int start = 0, string? queue = null)
    {
        if (!IsConfigured) return null;
        
        try
        {
            var url = $"https://{RegionalEndpoint}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start={start}&count={count}";
            if (!string.IsNullOrEmpty(queue))
            {
                url += $"&queue={queue}";
            }
            
            var response = await SendRequestAsync<List<string>>(url);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get match IDs");
            return null;
        }
    }
    
    /// <summary>
    /// Get detailed match data by match ID
    /// </summary>
    public async Task<RiotMatch?> GetMatchAsync(string matchId)
    {
        if (!IsConfigured) return null;
        
        try
        {
            var url = $"https://{RegionalEndpoint}.api.riotgames.com/lol/match/v5/matches/{matchId}";
            var response = await SendRequestAsync<RiotMatch>(url);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get match {MatchId}", matchId);
            return null;
        }
    }
    
    /// <summary>
    /// Get current ranked stats for a summoner
    /// </summary>
    public async Task<List<RiotLeagueEntry>?> GetRankedStatsAsync(string summonerId)
    {
        if (!IsConfigured) return null;
        
        try
        {
            var url = $"https://{Region}.api.riotgames.com/lol/league/v4/entries/by-summoner/{summonerId}";
            var response = await SendRequestAsync<List<RiotLeagueEntry>>(url);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ranked stats");
            return null;
        }
    }
    
    /// <summary>
    /// Test if the API key is valid
    /// </summary>
    public async Task<bool> TestApiKeyAsync()
    {
        if (!IsConfigured) return false;
        
        try
        {
            // Simple test - get platform status
            var url = $"https://{Region}.api.riotgames.com/lol/status/v4/platform-data";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Riot-Token", ApiKey);
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<T?> SendRequestAsync<T>(string url) where T : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Riot-Token", ApiKey);
        
        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Riot API request failed: {StatusCode} - {Error}", response.StatusCode, error);
            return null;
        }
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }
}

#region Riot API DTOs

public class RiotAccount
{
    [JsonProperty("puuid")]
    public string Puuid { get; set; } = string.Empty;
    
    [JsonProperty("gameName")]
    public string GameName { get; set; } = string.Empty;
    
    [JsonProperty("tagLine")]
    public string TagLine { get; set; } = string.Empty;
}

public class RiotSummoner
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("accountId")]
    public string AccountId { get; set; } = string.Empty;
    
    [JsonProperty("puuid")]
    public string Puuid { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("profileIconId")]
    public int ProfileIconId { get; set; }
    
    [JsonProperty("summonerLevel")]
    public long SummonerLevel { get; set; }
}

public class RiotLeagueEntry
{
    [JsonProperty("queueType")]
    public string QueueType { get; set; } = string.Empty;
    
    [JsonProperty("tier")]
    public string Tier { get; set; } = string.Empty;
    
    [JsonProperty("rank")]
    public string Rank { get; set; } = string.Empty;
    
    [JsonProperty("leaguePoints")]
    public int LeaguePoints { get; set; }
    
    [JsonProperty("wins")]
    public int Wins { get; set; }
    
    [JsonProperty("losses")]
    public int Losses { get; set; }
}

public class RiotMatch
{
    [JsonProperty("metadata")]
    public RiotMatchMetadata Metadata { get; set; } = new();
    
    [JsonProperty("info")]
    public RiotMatchInfo Info { get; set; } = new();
}

public class RiotMatchMetadata
{
    [JsonProperty("matchId")]
    public string MatchId { get; set; } = string.Empty;
    
    [JsonProperty("participants")]
    public List<string> Participants { get; set; } = new();
}

public class RiotMatchInfo
{
    [JsonProperty("gameId")]
    public long GameId { get; set; }
    
    [JsonProperty("queueId")]
    public int QueueId { get; set; }
    
    [JsonProperty("gameMode")]
    public string GameMode { get; set; } = string.Empty;
    
    [JsonProperty("gameDuration")]
    public long GameDuration { get; set; }
    
    [JsonProperty("gameCreation")]
    public long GameCreation { get; set; }
    
    [JsonProperty("gameEndTimestamp")]
    public long GameEndTimestamp { get; set; }
    
    [JsonProperty("participants")]
    public List<RiotParticipant> Participants { get; set; } = new();
}

public class RiotParticipant
{
    [JsonProperty("puuid")]
    public string Puuid { get; set; } = string.Empty;
    
    [JsonProperty("summonerId")]
    public string SummonerId { get; set; } = string.Empty;
    
    [JsonProperty("summonerName")]
    public string SummonerName { get; set; } = string.Empty;
    
    [JsonProperty("riotIdGameName")]
    public string RiotIdGameName { get; set; } = string.Empty;
    
    [JsonProperty("riotIdTagline")]
    public string RiotIdTagline { get; set; } = string.Empty;
    
    [JsonProperty("championId")]
    public int ChampionId { get; set; }
    
    [JsonProperty("championName")]
    public string ChampionName { get; set; } = string.Empty;
    
    [JsonProperty("teamId")]
    public int TeamId { get; set; }
    
    [JsonProperty("teamPosition")]
    public string TeamPosition { get; set; } = string.Empty;
    
    [JsonProperty("win")]
    public bool Win { get; set; }
    
    // Combat stats
    [JsonProperty("kills")]
    public int Kills { get; set; }
    
    [JsonProperty("deaths")]
    public int Deaths { get; set; }
    
    [JsonProperty("assists")]
    public int Assists { get; set; }
    
    // CS
    [JsonProperty("totalMinionsKilled")]
    public int TotalMinionsKilled { get; set; }
    
    [JsonProperty("neutralMinionsKilled")]
    public int NeutralMinionsKilled { get; set; }
    
    // Damage
    [JsonProperty("totalDamageDealt")]
    public int TotalDamageDealt { get; set; }
    
    [JsonProperty("totalDamageDealtToChampions")]
    public int TotalDamageDealtToChampions { get; set; }
    
    [JsonProperty("physicalDamageDealtToChampions")]
    public int PhysicalDamageDealtToChampions { get; set; }
    
    [JsonProperty("magicDamageDealtToChampions")]
    public int MagicDamageDealtToChampions { get; set; }
    
    [JsonProperty("trueDamageDealtToChampions")]
    public int TrueDamageDealtToChampions { get; set; }
    
    [JsonProperty("totalDamageTaken")]
    public int TotalDamageTaken { get; set; }
    
    [JsonProperty("damageDealtToBuildings")]
    public int DamageDealtToBuildings { get; set; }
    
    [JsonProperty("damageDealtToObjectives")]
    public int DamageDealtToObjectives { get; set; }
    
    [JsonProperty("damageDealtToTurrets")]
    public int DamageDealtToTurrets { get; set; }
    
    // Gold
    [JsonProperty("goldEarned")]
    public int GoldEarned { get; set; }
    
    [JsonProperty("goldSpent")]
    public int GoldSpent { get; set; }
    
    // Multi-kills
    [JsonProperty("doubleKills")]
    public int DoubleKills { get; set; }
    
    [JsonProperty("tripleKills")]
    public int TripleKills { get; set; }
    
    [JsonProperty("quadraKills")]
    public int QuadraKills { get; set; }
    
    [JsonProperty("pentaKills")]
    public int PentaKills { get; set; }
    
    [JsonProperty("largestKillingSpree")]
    public int LargestKillingSpree { get; set; }
    
    [JsonProperty("largestMultiKill")]
    public int LargestMultiKill { get; set; }
    
    [JsonProperty("killingSprees")]
    public int KillingSprees { get; set; }
    
    // Objectives
    [JsonProperty("turretKills")]
    public int TurretKills { get; set; }
    
    [JsonProperty("inhibitorKills")]
    public int InhibitorKills { get; set; }
    
    // First blood
    [JsonProperty("firstBloodKill")]
    public bool FirstBloodKill { get; set; }
    
    [JsonProperty("firstBloodAssist")]
    public bool FirstBloodAssist { get; set; }
    
    // Vision
    [JsonProperty("visionScore")]
    public int VisionScore { get; set; }
    
    [JsonProperty("wardsPlaced")]
    public int WardsPlaced { get; set; }
    
    [JsonProperty("wardsKilled")]
    public int WardsKilled { get; set; }
    
    [JsonProperty("visionWardsBoughtInGame")]
    public int VisionWardsBoughtInGame { get; set; }
    
    // Healing
    [JsonProperty("totalHeal")]
    public int TotalHeal { get; set; }
    
    [JsonProperty("totalHealsOnTeammates")]
    public int TotalHealsOnTeammates { get; set; }
    
    [JsonProperty("totalDamageShieldedOnTeammates")]
    public int TotalDamageShieldedOnTeammates { get; set; }
    
    // CC
    [JsonProperty("totalTimeCCDealt")]
    public int TotalTimeCCDealt { get; set; }
    
    [JsonProperty("timeCCingOthers")]
    public int TimeCCingOthers { get; set; }
    
    // Time
    [JsonProperty("totalTimeSpentDead")]
    public int TotalTimeSpentDead { get; set; }
    
    [JsonProperty("longestTimeSpentLiving")]
    public int LongestTimeSpentLiving { get; set; }
    
    // Level
    [JsonProperty("champLevel")]
    public int ChampLevel { get; set; }
    
    // Spells
    [JsonProperty("summoner1Id")]
    public int Summoner1Id { get; set; }
    
    [JsonProperty("summoner2Id")]
    public int Summoner2Id { get; set; }
    
    [JsonProperty("summoner1Casts")]
    public int Summoner1Casts { get; set; }
    
    [JsonProperty("summoner2Casts")]
    public int Summoner2Casts { get; set; }
    
    // Items
    [JsonProperty("item0")]
    public int Item0 { get; set; }
    
    [JsonProperty("item1")]
    public int Item1 { get; set; }
    
    [JsonProperty("item2")]
    public int Item2 { get; set; }
    
    [JsonProperty("item3")]
    public int Item3 { get; set; }
    
    [JsonProperty("item4")]
    public int Item4 { get; set; }
    
    [JsonProperty("item5")]
    public int Item5 { get; set; }
    
    [JsonProperty("item6")]
    public int Item6 { get; set; }
    
    // Perks
    [JsonProperty("perks")]
    public RiotPerks? Perks { get; set; }
    
    // Arena augments
    [JsonProperty("playerAugment1")]
    public int PlayerAugment1 { get; set; }
    
    [JsonProperty("playerAugment2")]
    public int PlayerAugment2 { get; set; }
    
    [JsonProperty("playerAugment3")]
    public int PlayerAugment3 { get; set; }
    
    [JsonProperty("playerAugment4")]
    public int PlayerAugment4 { get; set; }
    
    // Surrender
    [JsonProperty("gameEndedInSurrender")]
    public bool GameEndedInSurrender { get; set; }
    
    [JsonProperty("gameEndedInEarlySurrender")]
    public bool GameEndedInEarlySurrender { get; set; }
    
    public List<int> GetItems() => new() { Item0, Item1, Item2, Item3, Item4, Item5, Item6 };
}

public class RiotPerks
{
    [JsonProperty("statPerks")]
    public RiotStatPerks? StatPerks { get; set; }
    
    [JsonProperty("styles")]
    public List<RiotPerkStyle> Styles { get; set; } = new();
}

public class RiotStatPerks
{
    [JsonProperty("defense")]
    public int Defense { get; set; }
    
    [JsonProperty("flex")]
    public int Flex { get; set; }
    
    [JsonProperty("offense")]
    public int Offense { get; set; }
}

public class RiotPerkStyle
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty("style")]
    public int Style { get; set; }
    
    [JsonProperty("selections")]
    public List<RiotPerkSelection> Selections { get; set; } = new();
}

public class RiotPerkSelection
{
    [JsonProperty("perk")]
    public int Perk { get; set; }
    
    [JsonProperty("var1")]
    public int Var1 { get; set; }
    
    [JsonProperty("var2")]
    public int Var2 { get; set; }
    
    [JsonProperty("var3")]
    public int Var3 { get; set; }
}

#endregion
