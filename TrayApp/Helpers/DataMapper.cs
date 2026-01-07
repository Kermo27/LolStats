using LolStatsTracker.Shared.Models;
using LolStatsTracker.TrayApp.Models.Lcu;
using LolStatsTracker.TrayApp.Services;

namespace LolStatsTracker.TrayApp.Helpers;

public static class DataMapper
{
    private static ChampionDataService? _championDataService;
    
    public static void Initialize(ChampionDataService championDataService)
    {
        _championDataService = championDataService;
    }
    
    public static MatchEntry MapToMatchEntry(LcuEndOfGameStats eogStats, LcuQueueStats? soloRankedStats, LcuQueueStats? flexRankedStats, Guid profileId, LcuGame? gameDetails = null)
    {
        var localPlayer = eogStats.LocalPlayer;
        if (localPlayer == null)
            throw new ArgumentException("Local player data is missing");

        // Augment EOG stats with gameDetails from match history if available
        if (gameDetails != null)
        {
            Console.WriteLine("[DataMapper] Augmenting EOG stats with Match History data...");
            foreach (var participant in gameDetails.Participants)
            {
                // Find matching player in EOG stats
                var eogPlayer = eogStats.Teams
                    .SelectMany(t => t.Players)
                    .FirstOrDefault(p => p.TeamId == participant.TeamId && p.ChampionId == participant.ChampionId);

                if (eogPlayer != null)
                {
                    // Update role/lane info from match history
                    if (string.IsNullOrEmpty(eogPlayer.Stats.Position)) eogPlayer.Stats.Position = participant.Timeline.Lane;
                    if (string.IsNullOrEmpty(eogPlayer.Stats.Lane)) eogPlayer.Stats.Lane = participant.Timeline.Lane;
                    if (string.IsNullOrEmpty(eogPlayer.Stats.Role)) eogPlayer.Stats.Role = participant.Timeline.Role;
                    Console.WriteLine($"[DataMapper]   Augmented player ChampId {participant.ChampionId}: Lane={participant.Timeline.Lane}, Role={participant.Timeline.Role}");
                }
            }
        }
        
        // Find local player in the teams list for consistent data
        var playerInTeam = eogStats.Teams
            .SelectMany(t => t.Players)
            .FirstOrDefault(p => p.Puuid == localPlayer.Puuid || (p.TeamId == localPlayer.TeamId && p.ChampionId == localPlayer.ChampionId));
        
        // Use the one from the team list if found, otherwise fallback to localPlayer object
        var sourcePlayer = playerInTeam ?? localPlayer;
        
        var championName = GetChampionName(sourcePlayer.ChampionId);
        
        // Priority for position detection:
        // 1. DetectedTeamPosition (most reliable - detected by Riot)
        // 2. SelectedPosition (what player queued for)
        // 3. Stats.Position / Stats.Lane / Stats.Role
        var position = MapPositionFromPlayer(sourcePlayer);
        
        Console.WriteLine($"[DataMapper] Position detected for {championName}: {position} (DetectedTeamPosition={sourcePlayer.DetectedTeamPosition}, SelectedPosition={sourcePlayer.SelectedPosition})");


        // For Support: use VisionScore, for others: use CS
        var csOrVisionScore = position == "Support" 
            ? sourcePlayer.Stats.VisionScore 
            : sourcePlayer.Stats.MinionsKilled + sourcePlayer.Stats.NeutralMinionsKilled;
        var gameLengthMinutes = eogStats.GameLength / 60;
        
        // Find team result
        var playerTeam = eogStats.Teams.FirstOrDefault(t => t.Players.Any(p => p.TeamId == sourcePlayer.TeamId));
        var isWin = playerTeam?.IsWinningTeam ?? sourcePlayer.Stats.Win;
        
        // Detect lane participants for Summoner's Rift modes
        var gameMode = MapQueueIdToGameMode(eogStats.QueueId);
        var isSummonersRift = gameMode is "Ranked Solo" or "Ranked Flex" or "Normal";
        
        var (laneAlly, laneEnemy, laneEnemyAlly) = isSummonersRift 
            ? DetectLaneParticipants(eogStats, sourcePlayer, position)
            : ("", "", "");
        
        var stats = sourcePlayer.Stats;
        
        // Select appropriate ranked stats based on queue type
        // QueueId 420 = Ranked Solo/Duo, QueueId 440 = Ranked Flex
        var rankedStats = eogStats.QueueId == 440 ? flexRankedStats : soloRankedStats;
        
        var match = new MatchEntry
        {
            Id = Guid.NewGuid(),
            GameId = eogStats.GameId,
            ProfileId = profileId,
            Champion = championName,
            Role = isSummonersRift ? position : "N/A",
            LaneAlly = laneAlly,
            LaneEnemy = laneEnemy,
            LaneEnemyAlly = laneEnemyAlly,
            
            // Basic Combat Stats
            Kills = stats.Kills,
            Deaths = stats.Deaths,
            Assists = stats.Assists,
            Cs = csOrVisionScore,
            GameLengthMinutes = gameLengthMinutes,
            Win = isWin,
            Date = DateTime.UtcNow,
            
            // Rank Info - uses appropriate ranked stats based on queue type
            CurrentTier = rankedStats?.Tier ?? "Unranked",
            CurrentDivision = ParseDivision(rankedStats?.Division),
            CurrentLp = rankedStats?.LeaguePoints ?? 0,
            GameMode = gameMode,
            QueueId = eogStats.QueueId,
            
            // Damage Dealt
            TotalDamageDealt = stats.TotalDamageDealt,
            DamageDealtToChampions = stats.DamageDealtToChampions,
            PhysicalDamageDealt = stats.PhysicalDamageDealt,
            PhysicalDamageToChampions = stats.PhysicalDamageToChampions,
            MagicDamageDealt = stats.MagicDamageDealt,
            MagicDamageToChampions = stats.MagicDamageToChampions,
            TrueDamageDealt = stats.TrueDamageDealt,
            TrueDamageToChampions = stats.TrueDamageToChampions,
            DamageToBuildings = stats.DamageToBuildings,
            DamageToObjectives = stats.DamageToObjectives,
            DamageToTurrets = stats.DamageToTurrets,
            
            // Damage Taken
            TotalDamageTaken = stats.TotalDamageTaken,
            PhysicalDamageTaken = stats.PhysicalDamageTaken,
            MagicDamageTaken = stats.MagicDamageTaken,
            TrueDamageTaken = stats.TrueDamageTaken,
            DamageSelfMitigated = stats.DamageSelfMitigated,
            
            // Gold
            GoldEarned = stats.GoldEarned,
            GoldSpent = stats.GoldSpent,
            
            // Multi-Kills
            DoubleKills = stats.DoubleKills,
            TripleKills = stats.TripleKills,
            QuadraKills = stats.QuadraKills,
            PentaKills = stats.PentaKills,
            LargestKillingSpree = stats.LargestKillingSpree,
            LargestMultiKill = stats.LargestMultiKill,
            KillingSprees = stats.KillingSprees,
            
            // Objectives
            TurretsKilled = stats.TurretsKilled,
            InhibitorsKilled = stats.InhibitorsKilled,
            
            // First Blood
            FirstBloodKill = stats.FirstBloodKill,
            FirstBloodAssist = stats.FirstBloodAssist,
            
            // Vision
            VisionScore = stats.VisionScore,
            WardsPlaced = stats.WardsPlaced,
            WardsKilled = stats.WardsKilled,
            VisionWardsBought = stats.VisionWardsBought,
            
            // Healing & Shielding
            TotalHeal = stats.TotalHeal,
            HealOnTeammates = stats.HealOnTeammates,
            UnitsHealed = stats.UnitsHealed,
            DamageShieldedOnTeammates = stats.DamageShieldedOnTeammates,
            
            // Crowd Control
            TotalTimeCCDealt = stats.TotalTimeCCDealt,
            TimeCCingOthers = stats.TimeCCingOthers,
            
            // Time Stats
            TimeSpentDead = stats.TimeSpentDead,
            LongestTimeSpentLiving = stats.LongestTimeSpentLiving,
            
            // Combat Score
            LargestCriticalStrike = stats.LargestCriticalStrike,
            CombatPlayerScore = stats.CombatPlayerScore,
            TotalPlayerScore = stats.TotalPlayerScore,
            
            // Level
            ChampionLevel = stats.Level > 0 ? stats.Level : null,
            
            // Summoner Spells
            Spell1Id = sourcePlayer.Spell1Id,
            Spell2Id = sourcePlayer.Spell2Id,
            
            // Champion Ability Casts (Q, W, E, R)
            Spell1Casts = stats.Spell1Cast,
            Spell2Casts = stats.Spell2Cast,
            Spell3Casts = stats.Spell3Cast,
            Spell4Casts = stats.Spell4Cast,
            
            // Items (comma-separated IDs)
            ItemsBuild = sourcePlayer.Items.Any() 
                ? string.Join(",", sourcePlayer.Items.Where(i => i > 0)) 
                : null,
            
            // Perks (Runes)
            PerkPrimaryStyle = stats.PerkPrimaryStyle,
            PerkSubStyle = stats.PerkSubStyle,
            Perks = FormatPerks(stats),
            
            // Arena Augments
            Augment1 = sourcePlayer.PlayerAugment1 > 0 ? sourcePlayer.PlayerAugment1 : null,
            Augment2 = sourcePlayer.PlayerAugment2 > 0 ? sourcePlayer.PlayerAugment2 : null,
            Augment3 = sourcePlayer.PlayerAugment3 > 0 ? sourcePlayer.PlayerAugment3 : null,
            Augment4 = sourcePlayer.PlayerAugment4 > 0 ? sourcePlayer.PlayerAugment4 : null,
            
            // Surrender
            GameEndedInSurrender = stats.GameEndedInSurrender,
            GameEndedInEarlySurrender = stats.GameEndedInEarlySurrender
        };
        
        return match;
    }
    
    private static string? FormatPerks(LcuPlayerStats stats)
    {
        var perks = new[] { stats.Perk0, stats.Perk1, stats.Perk2, stats.Perk3, stats.Perk4, stats.Perk5 }
            .Where(p => p > 0)
            .ToList();
        return perks.Any() ? string.Join(",", perks) : null;
    }

    public static string MapQueueIdToGameMode(int queueId) => queueId switch
    {
        420 => "Ranked Solo",
        440 => "Ranked Flex",
        400 or 430 => "Normal",
        480 => "Swift Play",
        450 => "ARAM",
        2400 => "ARAM Mayhem",
        900 => "ARURF",
        1900 => "URF",
        1700 => "Arena",
        _ => "Other"
    };
    
    private static string GetChampionName(int championId)
    {
        return _championDataService?.GetChampionName(championId) ?? $"Champion_{championId}";
    }
    
    private static string MapPositionFromPlayer(LcuPlayer player)
    {
        // Priority 1: DetectedTeamPosition (most reliable - auto-detected by Riot)
        var detected = NormalizePosition(player.DetectedTeamPosition);
        if (!string.IsNullOrEmpty(detected)) return detected;
        
        // Priority 2: SelectedPosition (what player queued for)
        var selected = NormalizePosition(player.SelectedPosition);
        if (!string.IsNullOrEmpty(selected)) return selected;
        
        // Priority 3: Stats fields
        var fromStats = NormalizePosition(player.Stats.Position) 
                     ?? NormalizePosition(player.Stats.Lane) 
                     ?? NormalizePosition(player.Stats.Role)
                     ?? NormalizePosition(player.Stats.PlayerPosition);
        if (!string.IsNullOrEmpty(fromStats)) return fromStats;
        
        // No fallback - return empty to indicate unknown (will show in logs)
        return "Unknown";
    }
    
    private static string? NormalizePosition(string? pos)
    {
        if (string.IsNullOrWhiteSpace(pos)) return null;
        
        var upper = pos.ToUpperInvariant();
        if (upper == "NONE" || upper == "NULL" || upper == "UNKNOWN" || upper == "") return null;
        
        if (upper == "TOP") return "Top";
        if (upper == "JUNGLE") return "Jungle";
        if (upper == "MIDDLE" || upper == "MID") return "Mid";
        if (upper == "BOTTOM" || upper == "BOT" || upper == "ADC" || upper == "CARRY") return "ADC";
        if (upper == "UTILITY" || upper == "SUPPORT") return "Support";
        
        return null;
    }

    private static bool IsNoneOrEmpty(string? s) => string.IsNullOrEmpty(s) || IsNone(s);

    private static bool IsNone(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return true;
        var upper = s.ToUpperInvariant();
        return upper == "NONE" || upper == "NULL" || upper == "UNKNOWN";
    }

    private static bool IsCommonAdc(int championId)
    {
        // Common ADCs - based on ID
        int[] adcs = { 
            15, 18, 21, 22, 29, 42, 51, 67, 81, 96, 110, 119, 133, 145, 202, 
            221, 222, 235, 236, 360, 429, 523, 895, 901 
        };
        return adcs.Contains(championId);
    }
    
    private static int ParseDivision(string? division)
    {
        return division switch
        {
            "I" => 1,
            "II" => 2,
            "III" => 3,
            "IV" => 4,
            _ => 4
        };
    }

    private static (string laneAlly, string laneEnemy, string laneEnemyAlly) DetectLaneParticipants(
        LcuEndOfGameStats eogStats, LcuPlayer localPlayer, string position)
    {
        var laneAlly = "";
        var laneEnemy = "";
        var laneEnemyAlly = "";
        
        var sourcePlayer = eogStats.Teams
            .SelectMany(t => t.Players)
            .FirstOrDefault(p => p.Puuid == localPlayer.Puuid || 
                (p.TeamId == localPlayer.TeamId && p.ChampionId == localPlayer.ChampionId)) ?? localPlayer;

        Console.WriteLine($"[DataMapper] Detecting lane participants for {position}");
        
        var allyTeam = eogStats.Teams.FirstOrDefault(t => t.Players.Any(p => p.TeamId == sourcePlayer.TeamId));
        var enemyTeam = eogStats.Teams.FirstOrDefault(t => !t.Players.Any(p => p.TeamId == sourcePlayer.TeamId));
        
        if (allyTeam == null || enemyTeam == null)
        {
            Console.WriteLine("[DataMapper] Could not find teams");
            return (laneAlly, laneEnemy, laneEnemyAlly);
        }
        
        // Define what role we're looking for based on our position
        var (targetAllyRole, targetEnemyRole, targetEnemyAllyRole) = position switch
        {
            "Top" => ("JUNGLE", "TOP", "JUNGLE"),           // Ally: Jungler, Enemy: Top, EnemyAlly: Jungler
            "Jungle" => ("MIDDLE", "JUNGLE", "MIDDLE"),      // Ally: Mid, Enemy: Jungle, EnemyAlly: Mid
            "Mid" => ("JUNGLE", "MIDDLE", "JUNGLE"),         // Ally: Jungler, Enemy: Mid, EnemyAlly: Jungler
            "ADC" => ("UTILITY", "BOTTOM", "UTILITY"),       // Ally: Support, Enemy: ADC, EnemyAlly: Support
            "Support" => ("BOTTOM", "UTILITY", "BOTTOM"),    // Ally: ADC, Enemy: Support, EnemyAlly: ADC
            _ => ("", "", "")
        };
        
        if (string.IsNullOrEmpty(targetAllyRole))
        {
            Console.WriteLine($"[DataMapper] Unknown position: {position}");
            return (laneAlly, laneEnemy, laneEnemyAlly);
        }
        
        // Find players by their detected position - prioritize DetectedTeamPosition, then Stats fields
        var allyPlayer = FindPlayerByDetectedPosition(allyTeam.Players, targetAllyRole, localPlayer.ChampionId);
        var enemyPlayer = FindPlayerByDetectedPosition(enemyTeam.Players, targetEnemyRole, -1);
        var enemyAllyPlayer = FindPlayerByDetectedPosition(enemyTeam.Players, targetEnemyAllyRole, -1);
        
        if (allyPlayer != null && allyPlayer.ChampionId != localPlayer.ChampionId)
            laneAlly = GetChampionName(allyPlayer.ChampionId);
        if (enemyPlayer != null)
            laneEnemy = GetChampionName(enemyPlayer.ChampionId);
        if (enemyAllyPlayer != null && enemyAllyPlayer.ChampionId != enemyPlayer?.ChampionId)
            laneEnemyAlly = GetChampionName(enemyAllyPlayer.ChampionId);
        
        Console.WriteLine($"[DataMapper] Result - LaneAlly: '{laneAlly}', LaneEnemy: '{laneEnemy}', LaneEnemyAlly: '{laneEnemyAlly}'");
        
        return (laneAlly, laneEnemy, laneEnemyAlly);
    }
    
    private static LcuPlayer? FindPlayerByDetectedPosition(List<LcuPlayer> players, string targetRole, int excludeChampionId)
    {
        // Priority 1: DetectedTeamPosition field (most reliable)
        var found = players.FirstOrDefault(p => 
            p.ChampionId != excludeChampionId &&
            MatchesRole(p.DetectedTeamPosition, targetRole));
        
        if (found != null) return found;
        
        // Priority 2: SelectedPosition field  
        found = players.FirstOrDefault(p => 
            p.ChampionId != excludeChampionId &&
            MatchesRole(p.SelectedPosition, targetRole));
        
        if (found != null) return found;
        
        // Priority 3: Stats position fields
        found = players.FirstOrDefault(p => 
            p.ChampionId != excludeChampionId &&
            (MatchesRole(p.Stats.Position, targetRole) || 
             MatchesRole(p.Stats.Lane, targetRole) || 
             MatchesRole(p.Stats.Role, targetRole)));
        
        return found; // May be null - no fallback to random index
    }
    
    private static bool MatchesRole(string? playerRole, string targetRole)
    {
        if (string.IsNullOrWhiteSpace(playerRole) || string.IsNullOrWhiteSpace(targetRole))
            return false;
        
        var upper = playerRole.ToUpperInvariant();
        return targetRole switch
        {
            "TOP" => upper == "TOP",
            "JUNGLE" => upper == "JUNGLE",
            "MIDDLE" => upper == "MIDDLE" || upper == "MID",
            "BOTTOM" => upper == "BOTTOM" || upper == "BOT" || upper == "ADC" || upper == "CARRY",
            "UTILITY" => upper == "UTILITY" || upper == "SUPPORT",
            _ => false
        };
    }
}
