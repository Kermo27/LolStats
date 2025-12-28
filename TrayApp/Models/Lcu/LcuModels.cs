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

    [JsonProperty("gameName")]
    public string GameName { get; set; } = string.Empty;

    [JsonProperty("tagLine")]
    public string TagLine { get; set; } = string.Empty;
    
    [JsonProperty("summonerId")]
    public long SummonerId { get; set; }
    
    [JsonProperty("puuid")]
    public string Puuid { get; set; } = string.Empty;
    
    [JsonProperty("championId")]
    public int ChampionId { get; set; }
    
    [JsonProperty("teamId")]
    public int TeamId { get; set; }
    
    [JsonProperty("stats")]
    public LcuPlayerStats Stats { get; set; } = new();
    
    // Items (final build)
    [JsonProperty("items")]
    public List<int> Items { get; set; } = new();
    
    // Summoner Spells
    [JsonProperty("spell1Id")]
    public int Spell1Id { get; set; }
    
    [JsonProperty("spell2Id")]
    public int Spell2Id { get; set; }
    
    // Level
    [JsonProperty("level")]
    public int Level { get; set; }
    
    // Arena Augments
    [JsonProperty("playerAugment1")]
    public int PlayerAugment1 { get; set; }
    
    [JsonProperty("playerAugment2")]
    public int PlayerAugment2 { get; set; }
    
    [JsonProperty("playerAugment3")]
    public int PlayerAugment3 { get; set; }
    
    [JsonProperty("playerAugment4")]
    public int PlayerAugment4 { get; set; }
    
    // Detected Team Position (from eog)
    [JsonProperty("detectedTeamPosition")]
    public string DetectedTeamPosition { get; set; } = string.Empty;
    
    // Selected Position
    [JsonProperty("selectedPosition")]
    public string SelectedPosition { get; set; } = string.Empty;

    [JsonIgnore]
    public string DisplayName => !string.IsNullOrEmpty(GameName) ? $"{GameName}#{TagLine}" : SummonerName;
}

public class LcuPlayerStats
{
    // Basic Combat Stats
    [JsonProperty("CHAMPIONS_KILLED")]
    public int Kills { get; set; }
    
    [JsonProperty("NUM_DEATHS")]
    public int Deaths { get; set; }
    
    [JsonProperty("ASSISTS")]
    public int Assists { get; set; }
    
    // Minions & Farming
    [JsonProperty("MINIONS_KILLED")]
    public int MinionsKilled { get; set; }
    
    [JsonProperty("NEUTRAL_MINIONS_KILLED")]
    public int NeutralMinionsKilled { get; set; }
    
    // Vision
    [JsonProperty("VISION_SCORE")]
    public int VisionScore { get; set; }
    
    [JsonProperty("WARD_PLACED")]
    public int WardsPlaced { get; set; }
    
    [JsonProperty("WARD_KILLED")]
    public int WardsKilled { get; set; }
    
    [JsonProperty("VISION_WARDS_BOUGHT_IN_GAME")]
    public int VisionWardsBought { get; set; }
    
    [JsonProperty("SIGHT_WARDS_BOUGHT_IN_GAME")]
    public int SightWardsBought { get; set; }
    
    // Position & Role
    [JsonProperty("INDIVIDUAL_POSITION")]
    public string Position { get; set; } = string.Empty;

    [JsonProperty("LANE")]
    public string Lane { get; set; } = string.Empty;

    [JsonProperty("ROLE")]
    public string Role { get; set; } = string.Empty;

    [JsonProperty("PLAYER_POSITION")]
    public string PlayerPosition { get; set; } = string.Empty;
    
    [JsonProperty("WIN")]
    public bool Win { get; set; }
    
    // Damage Dealt
    [JsonProperty("TOTAL_DAMAGE_DEALT")]
    public int TotalDamageDealt { get; set; }
    
    [JsonProperty("TOTAL_DAMAGE_DEALT_TO_CHAMPIONS")]
    public int DamageDealtToChampions { get; set; }
    
    [JsonProperty("PHYSICAL_DAMAGE_DEALT_PLAYER")]
    public int PhysicalDamageDealt { get; set; }
    
    [JsonProperty("PHYSICAL_DAMAGE_DEALT_TO_CHAMPIONS")]
    public int PhysicalDamageToChampions { get; set; }
    
    [JsonProperty("MAGIC_DAMAGE_DEALT_PLAYER")]
    public int MagicDamageDealt { get; set; }
    
    [JsonProperty("MAGIC_DAMAGE_DEALT_TO_CHAMPIONS")]
    public int MagicDamageToChampions { get; set; }
    
    [JsonProperty("TRUE_DAMAGE_DEALT_PLAYER")]
    public int TrueDamageDealt { get; set; }
    
    [JsonProperty("TRUE_DAMAGE_DEALT_TO_CHAMPIONS")]
    public int TrueDamageToChampions { get; set; }
    
    [JsonProperty("TOTAL_DAMAGE_DEALT_TO_BUILDINGS")]
    public int DamageToBuildings { get; set; }
    
    [JsonProperty("TOTAL_DAMAGE_DEALT_TO_OBJECTIVES")]
    public int DamageToObjectives { get; set; }
    
    [JsonProperty("TOTAL_DAMAGE_DEALT_TO_TURRETS")]
    public int DamageToTurrets { get; set; }
    
    // Damage Taken
    [JsonProperty("TOTAL_DAMAGE_TAKEN")]
    public int TotalDamageTaken { get; set; }
    
    [JsonProperty("PHYSICAL_DAMAGE_TAKEN")]
    public int PhysicalDamageTaken { get; set; }
    
    [JsonProperty("MAGIC_DAMAGE_TAKEN")]
    public int MagicDamageTaken { get; set; }
    
    [JsonProperty("TRUE_DAMAGE_TAKEN")]
    public int TrueDamageTaken { get; set; }
    
    [JsonProperty("TOTAL_DAMAGE_SELF_MITIGATED")]
    public int DamageSelfMitigated { get; set; }
    
    // Gold
    [JsonProperty("GOLD_EARNED")]
    public int GoldEarned { get; set; }
    
    [JsonProperty("GOLD_SPENT")]
    public int GoldSpent { get; set; }
    
    // Multi-Kills
    [JsonProperty("DOUBLE_KILLS")]
    public int DoubleKills { get; set; }
    
    [JsonProperty("TRIPLE_KILLS")]
    public int TripleKills { get; set; }
    
    [JsonProperty("QUADRA_KILLS")]
    public int QuadraKills { get; set; }
    
    [JsonProperty("PENTA_KILLS")]
    public int PentaKills { get; set; }
    
    [JsonProperty("LARGEST_KILLING_SPREE")]
    public int LargestKillingSpree { get; set; }
    
    [JsonProperty("LARGEST_MULTI_KILL")]
    public int LargestMultiKill { get; set; }
    
    [JsonProperty("KILLING_SPREES")]
    public int KillingSprees { get; set; }
    
    // Objectives
    [JsonProperty("TURRETS_KILLED")]
    public int TurretsKilled { get; set; }
    
    [JsonProperty("BARRACKS_KILLED")]
    public int InhibitorsKilled { get; set; }
    
    // First Blood
    [JsonProperty("FIRST_BLOOD_KILL")]
    public bool FirstBloodKill { get; set; }
    
    [JsonProperty("FIRST_BLOOD_ASSIST")]
    public bool FirstBloodAssist { get; set; }
    
    // Healing & Shielding
    [JsonProperty("TOTAL_HEAL")]
    public int TotalHeal { get; set; }
    
    [JsonProperty("TOTAL_HEAL_ON_TEAMMATES")]
    public int HealOnTeammates { get; set; }
    
    [JsonProperty("TOTAL_UNITS_HEALED")]
    public int UnitsHealed { get; set; }
    
    [JsonProperty("TOTAL_DAMAGE_SHIELDED_ON_TEAMMATES")]
    public int DamageShieldedOnTeammates { get; set; }
    
    // Crowd Control
    [JsonProperty("TOTAL_TIME_CROWD_CONTROL_DEALT")]
    public int TotalTimeCCDealt { get; set; }
    
    [JsonProperty("TIME_CCING_OTHERS")]
    public int TimeCCingOthers { get; set; }
    
    // Time Stats
    [JsonProperty("TIME_SPENT_DEAD")]
    public int TimeSpentDead { get; set; }
    
    [JsonProperty("LONGEST_TIME_SPENT_LIVING")]
    public int LongestTimeSpentLiving { get; set; }
    
    // Combat Score
    [JsonProperty("LARGEST_CRITICAL_STRIKE")]
    public int LargestCriticalStrike { get; set; }
    
    [JsonProperty("COMBAT_PLAYER_SCORE")]
    public int CombatPlayerScore { get; set; }
    
    [JsonProperty("OBJECTIVE_PLAYER_SCORE")]
    public int ObjectivePlayerScore { get; set; }
    
    [JsonProperty("TOTAL_PLAYER_SCORE")]
    public int TotalPlayerScore { get; set; }
    
    // Level
    [JsonProperty("LEVEL")]
    public int Level { get; set; }
    
    // Spell Casts
    [JsonProperty("SPELL1_CAST")]
    public int Spell1Cast { get; set; }
    
    [JsonProperty("SPELL2_CAST")]
    public int Spell2Cast { get; set; }
    
    // Surrender / Early
    [JsonProperty("GAME_ENDED_IN_SURRENDER")]
    public bool GameEndedInSurrender { get; set; }
    
    [JsonProperty("GAME_ENDED_IN_EARLY_SURRENDER")]
    public bool GameEndedInEarlySurrender { get; set; }
    
    [JsonProperty("TEAM_EARLY_SURRENDERED")]
    public bool TeamEarlySurrendered { get; set; }
    
    // Perks (Runes)
    [JsonProperty("PERK_PRIMARY_STYLE")]
    public int PerkPrimaryStyle { get; set; }
    
    [JsonProperty("PERK_SUB_STYLE")]
    public int PerkSubStyle { get; set; }
    
    [JsonProperty("PERK0")]
    public int Perk0 { get; set; }
    
    [JsonProperty("PERK1")]
    public int Perk1 { get; set; }
    
    [JsonProperty("PERK2")]
    public int Perk2 { get; set; }
    
    [JsonProperty("PERK3")]
    public int Perk3 { get; set; }
    
    [JsonProperty("PERK4")]
    public int Perk4 { get; set; }
    
    [JsonProperty("PERK5")]
    public int Perk5 { get; set; }
}

public class LcuGame
{
    [JsonProperty("gameId")]
    public long GameId { get; set; }
    
    [JsonProperty("queueId")]
    public int QueueId { get; set; }
    
    [JsonProperty("gameType")]
    public string GameType { get; set; } = string.Empty;

    [JsonProperty("participants")]
    public List<LcuGameParticipant> Participants { get; set; } = new();

    [JsonProperty("participantIdentities")]
    public List<LcuGameParticipantIdentity> ParticipantIdentities { get; set; } = new();
}

public class LcuGameParticipant
{
    [JsonProperty("participantId")]
    public int ParticipantId { get; set; }

    [JsonProperty("teamId")]
    public int TeamId { get; set; }

    [JsonProperty("championId")]
    public int ChampionId { get; set; }

    [JsonProperty("timeline")]
    public LcuGameTimeline Timeline { get; set; } = new();
}

public class LcuGameTimeline
{
    [JsonProperty("lane")]
    public string Lane { get; set; } = string.Empty;

    [JsonProperty("role")]
    public string Role { get; set; } = string.Empty;
}

public class LcuGameParticipantIdentity
{
    [JsonProperty("participantId")]
    public int ParticipantId { get; set; }

    [JsonProperty("player")]
    public LcuGamePlayer Player { get; set; } = new();
}

public class LcuGamePlayer
{
    [JsonProperty("summonerName")]
    public string SummonerName { get; set; } = string.Empty;

    [JsonProperty("gameName")]
    public string GameName { get; set; } = string.Empty;

    [JsonProperty("tagLine")]
    public string TagLine { get; set; } = string.Empty;

    [JsonProperty("puuid")]
    public string Puuid { get; set; } = string.Empty;
}
