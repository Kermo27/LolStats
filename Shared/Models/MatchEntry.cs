using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LolStatsTracker.Shared.Helpers;

namespace LolStatsTracker.Shared.Models;

public class MatchEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Game Identifier for deduplication
    public long? GameId { get; set; }
    
    [Required]
    public string Champion { get; set; } = string.Empty;
    public string Role { get; set; } = "N/A";
    public string LaneAlly { get; set; } = string.Empty;
    public string LaneEnemy { get; set; } = string.Empty;
    public string LaneEnemyAlly { get; set; } = string.Empty;
    
    // Basic Combat Stats
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Cs { get; set; }
    public int GameLengthMinutes { get; set; }
    public bool Win { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    
    // Rank Info
    public string CurrentTier { get; set; } = "Unranked";
    public int CurrentDivision { get; set; } = 4;
    public int CurrentLp { get; set; }
    public string GameMode { get; set; } = "Ranked Solo";
    public int QueueId { get; set; } = 420;
    public string? Notes { get; set; }
    public Guid? ProfileId { get; set; }
    
    // === EXTENDED STATS (Nullable for backward compatibility) ===
    
    // Damage Dealt
    public int? TotalDamageDealt { get; set; }
    public int? DamageDealtToChampions { get; set; }
    public int? PhysicalDamageDealt { get; set; }
    public int? PhysicalDamageToChampions { get; set; }
    public int? MagicDamageDealt { get; set; }
    public int? MagicDamageToChampions { get; set; }
    public int? TrueDamageDealt { get; set; }
    public int? TrueDamageToChampions { get; set; }
    public int? DamageToBuildings { get; set; }
    public int? DamageToObjectives { get; set; }
    public int? DamageToTurrets { get; set; }
    
    // Damage Taken
    public int? TotalDamageTaken { get; set; }
    public int? PhysicalDamageTaken { get; set; }
    public int? MagicDamageTaken { get; set; }
    public int? TrueDamageTaken { get; set; }
    public int? DamageSelfMitigated { get; set; }
    
    // Gold
    public int? GoldEarned { get; set; }
    public int? GoldSpent { get; set; }
    
    // Multi-Kills
    public int? DoubleKills { get; set; }
    public int? TripleKills { get; set; }
    public int? QuadraKills { get; set; }
    public int? PentaKills { get; set; }
    public int? LargestKillingSpree { get; set; }
    public int? LargestMultiKill { get; set; }
    public int? KillingSprees { get; set; }
    
    // Objectives
    public int? TurretsKilled { get; set; }
    public int? InhibitorsKilled { get; set; }
    
    // First Blood
    public bool? FirstBloodKill { get; set; }
    public bool? FirstBloodAssist { get; set; }
    
    // Vision
    public int? VisionScore { get; set; }
    public int? WardsPlaced { get; set; }
    public int? WardsKilled { get; set; }
    public int? VisionWardsBought { get; set; }
    
    // Healing & Shielding
    public int? TotalHeal { get; set; }
    public int? HealOnTeammates { get; set; }
    public int? UnitsHealed { get; set; }
    public int? DamageShieldedOnTeammates { get; set; }
    
    // Crowd Control
    public int? TotalTimeCCDealt { get; set; }
    public int? TimeCCingOthers { get; set; }
    
    // Time Stats
    public int? TimeSpentDead { get; set; }
    public int? LongestTimeSpentLiving { get; set; }
    
    // Combat Score
    public int? LargestCriticalStrike { get; set; }
    public int? CombatPlayerScore { get; set; }
    public int? TotalPlayerScore { get; set; }
    
    // Level
    public int? ChampionLevel { get; set; }
    
    // Summoner Spells
    public int? Spell1Id { get; set; }  // e.g. Flash
    public int? Spell2Id { get; set; }  // e.g. Heal
    
    // Champion Ability Casts (Q, W, E, R)
    public int? Spell1Casts { get; set; }  // Q
    public int? Spell2Casts { get; set; }  // W
    public int? Spell3Casts { get; set; }  // E
    public int? Spell4Casts { get; set; }  // R
    
    // Items (stored as comma-separated IDs)
    public string? ItemsBuild { get; set; }
    
    // Perks (Runes)
    public int? PerkPrimaryStyle { get; set; }
    public int? PerkSubStyle { get; set; }
    public string? Perks { get; set; } // Comma-separated perk IDs
    
    // Arena Augments
    public int? Augment1 { get; set; }
    public int? Augment2 { get; set; }
    public int? Augment3 { get; set; }
    public int? Augment4 { get; set; }
    
    // Surrender
    public bool? GameEndedInSurrender { get; set; }
    public bool? GameEndedInEarlySurrender { get; set; }

    // === COMPUTED PROPERTIES ===
    
    [NotMapped]
    public bool IsSummonersRift => GameMode is "Ranked Solo" or "Ranked Flex" or "Normal";
    
    [NotMapped]
    public string KdaDisplay => $"{Kills}/{Deaths}/{Assists}";
    
    [NotMapped]
    public int PerformanceScore => PerformanceScoreHelper.Calculate(Kills, Deaths, Assists, Cs, GameLengthMinutes, Win, Role);
    
    [NotMapped]
    public string PerformanceRating => PerformanceScoreHelper.GetRating(PerformanceScore);
    
    [NotMapped]
    public double Kda => Deaths == 0 ? Kills + Assists : (double)(Kills + Assists) / Deaths;
    
    [NotMapped]
    public double CsPerMinute => GameLengthMinutes > 0 ? (double)Cs / GameLengthMinutes : 0;
    
    [NotMapped]
    public double GoldPerMinute => GoldEarned.HasValue && GameLengthMinutes > 0 
        ? (double)GoldEarned.Value / GameLengthMinutes : 0;
    
    [NotMapped]
    public double DamagePerMinute => DamageDealtToChampions.HasValue && GameLengthMinutes > 0 
        ? (double)DamageDealtToChampions.Value / GameLengthMinutes : 0;
}