using Newtonsoft.Json;

namespace LolStatsTracker.TrayApp.Models.Lcu;

// LCU API Response Models

public class LcuSummoner
{
    [JsonProperty("summonerId")]
    public long SummonerId { get; set; }
    
    [JsonProperty("gameName")]
    public string GameName { get; set; } = string.Empty;
    
    [JsonProperty("tagLine")]
    public string TagLine { get; set; } = string.Empty;
    
    [JsonProperty("puuid")]
    public string Puuid { get; set; } = string.Empty;
    
    [JsonProperty("summonerLevel")]
    public int SummonerLevel { get; set; }
}

public class LcuRankedStats
{
    [JsonProperty("queues")]
    public List<LcuQueueStats> Queues { get; set; } = new();
}

public class LcuQueueStats
{
    [JsonProperty("queueType")]
    public string QueueType { get; set; } = string.Empty;
    
    [JsonProperty("tier")]
    public string Tier { get; set; } = string.Empty;
    
    [JsonProperty("division")]
    public string Division { get; set; } = string.Empty;
    
    [JsonProperty("leaguePoints")]
    public int LeaguePoints { get; set; }
    
    [JsonProperty("wins")]
    public int Wins { get; set; }
    
    [JsonProperty("losses")]
    public int Losses { get; set; }
}

public class LcuEndOfGameStats
{
    [JsonProperty("gameId")]
    public long GameId { get; set; }
    
    [JsonProperty("queueId")]
    public int QueueId { get; set; }
    
    [JsonProperty("gameLength")]
    public int GameLength { get; set; }
    
    [JsonProperty("teams")]
    public List<LcuTeam> Teams { get; set; } = new();
    
    [JsonProperty("localPlayer")]
    public LcuPlayer? LocalPlayer { get; set; }
}

public class LcuTeam
{
    [JsonProperty("isWinningTeam")]
    public bool IsWinningTeam { get; set; }
    
    [JsonProperty("players")]
    public List<LcuPlayer> Players { get; set; } = new();
}

public class LcuPlayer
{
    [JsonProperty("summonerName")]
    public string SummonerName { get; set; } = string.Empty;
    
    [JsonProperty("championId")]
    public int ChampionId { get; set; }
    
    [JsonProperty("teamId")]
    public int TeamId { get; set; }
    
    [JsonProperty("stats")]
    public LcuPlayerStats Stats { get; set; } = new();
}

public class LcuPlayerStats
{
    [JsonProperty("CHAMPIONS_KILLED")]
    public int Kills { get; set; }
    
    [JsonProperty("NUM_DEATHS")]
    public int Deaths { get; set; }
    
    [JsonProperty("ASSISTS")]
    public int Assists { get; set; }
    
    [JsonProperty("MINIONS_KILLED")]
    public int MinionsKilled { get; set; }
    
    [JsonProperty("NEUTRAL_MINIONS_KILLED")]
    public int NeutralMinionsKilled { get; set; }
    
    [JsonProperty("INDIVIDUAL_POSITION")]
    public string Position { get; set; } = string.Empty;
    
    [JsonProperty("WIN")]
    public bool Win { get; set; }
}

public class LcuGame
{
    [JsonProperty("gameId")]
    public long GameId { get; set; }
    
    [JsonProperty("queueId")]
    public int QueueId { get; set; }
    
    [JsonProperty("gameType")]
    public string GameType { get; set; } = string.Empty;
}
