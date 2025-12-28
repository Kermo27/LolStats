using System.Linq;
using LolStatsTracker.Shared.Models;
using LolStatsTracker.TrayApp.Models.Lcu;

namespace LolStatsTracker.TrayApp.Helpers;

public static class DataMapper
{
    private static readonly Dictionary<int, string> ChampionIdToName = new()
    {
        // Top champions - simplified mapping, można rozszerzyć
        { 1, "Annie" }, { 2, "Olaf" }, { 3, "Galio" }, { 4, "Twisted Fate" },
        { 5, "Xin Zhao" }, { 6, "Urgot" }, { 7, "LeBlanc" }, { 8, "Vladimir" },
        { 9, "Fiddlesticks" }, { 10, "Kayle" }, { 11, "Master Yi" }, { 12, "Alistar" },
        { 13, "Ryze" }, { 14, "Sion" }, { 15, "Sivir" }, { 16, "Soraka" },
        { 17, "Teemo" }, { 18, "Tristana" }, { 19, "Warwick" }, { 20, "Nunu" },
        { 21, "Miss Fortune" }, { 22, "Ashe" }, { 23, "Tryndamere" }, { 24, "Jax" },
        { 25, "Morgana" }, { 26, "Zilean" }, { 27, "Singed" }, { 28, "Evelynn" },
        { 29, "Twitch" }, { 30, "Karthus" }, { 31, "Cho'Gath" }, { 32, "Amumu" },
        { 33, "Rammus" }, { 34, "Anivia" }, { 35, "Shaco" }, { 36, "Dr. Mundo" },
        { 37, "Sona" }, { 38, "Kassadin" }, { 39, "Irelia" }, { 40, "Janna" },
        { 41, "Gangplank" }, { 42, "Corki" }, { 43, "Karma" }, { 44, "Taric" },
        { 45, "Veigar" }, { 48, "Trundle" }, { 50, "Swain" }, { 51, "Caitlyn" },
        { 53, "Blitzcrank" }, { 54, "Malphite" }, { 55, "Katarina" }, { 56, "Nocturne" },
        { 57, "Maokai" }, { 58, "Renekton" }, { 59, "Jarvan IV" }, { 60, "Elise" },
        { 61, "Orianna" }, { 62, "Wukong" }, { 63, "Brand" }, { 64, "Lee Sin" },
        { 67, "Vayne" }, { 68, "Rumble" }, { 69, "Cassiopeia" }, { 72, "Skarner" },
        { 74, "Heimerdinger" }, { 75, "Nasus" }, { 76, "Nidalee" }, { 77, "Udyr" },
        { 78, "Poppy" }, { 79, "Gragas" }, { 80, "Pantheon" }, { 81, "Ezreal" },
        { 82, "Mordekaiser" }, { 83, "Yorick" }, { 84, "Akali" }, { 85, "Kennen" },
        { 86, "Garen" }, { 89, "Leona" }, { 90, "Malzahar" }, { 91, "Talon" },
        { 92, "Riven" }, { 96, "Kog'Maw" }, { 98, "Shen" }, { 99, "Lux" },
        { 101, "Xerath" }, { 102, "Shyvana" }, { 103, "Ahri" }, { 104, "Graves" },
        { 105, "Fizz" }, { 106, "Volibear" }, { 107, "Rengar" }, { 110, "Varus" },
        { 111, "Nautilus" }, { 112, "Viktor" }, { 113, "Sejuani" }, { 114, "Fiora" },
        { 115, "Ziggs" }, { 117, "Lulu" }, { 119, "Draven" }, { 120, "Hecarim" },
        { 121, "Kha'Zix" }, { 122, "Darius" }, { 126, "Jayce" }, { 127, "Lissandra" },
        { 131, "Diana" }, { 133, "Quinn" }, { 134, "Syndra" }, { 136, "Aurelion Sol" },
        { 141, "Kayn" }, { 142, "Zoe" }, { 143, "Zyra" }, { 145, "Kai'Sa" },
        { 147, "Seraphine" }, { 150, "Gnar" }, { 154, "Zac" }, { 157, "Yasuo" },
        { 161, "Vel'Koz" }, { 163, "Taliyah" }, { 164, "Camille" }, { 166, "Akshan" },
        { 200, "Bel'Veth" }, { 201, "Braum" }, { 202, "Jhin" }, { 203, "Kindred" },
        { 221, "Zeri" }, { 222, "Jinx" }, { 223, "Tahm Kench" }, { 234, "Viego" },
        { 235, "Senna" }, { 236, "Lucian" }, { 238, "Zed" }, { 240, "Kled" },
        { 245, "Ekko" }, { 246, "Qiyana" }, { 254, "Vi" }, { 266, "Aatrox" },
        { 267, "Nami" }, { 268, "Azir" }, { 350, "Yuumi" }, { 360, "Samira" },
        { 412, "Thresh" }, { 420, "Illaoi" }, { 421, "Rek'Sai" }, { 427, "Ivern" },
        { 429, "Kalista" }, { 432, "Bard" }, { 516, "Ornn" }, { 517, "Sylas" },
        { 518, "Neeko" }, { 523, "Aphelios" }, { 526, "Rell" }, { 555, "Pyke" },
        { 711, "Vex" }, { 777, "Yone" }, { 875, "Sett" }, { 876, "Lillia" },
        { 887, "Gwen" }, { 888, "Renata Glasc" }, { 895, "Nilah" }, { 897, "K'Sante" },
        { 902, "Milio" }, { 910, "Hwei" }, { 950, "Naafiri" }, { 901, "Smolder" },
        { 233, "Briar" }
    };
    
    public static MatchEntry MapToMatchEntry(LcuEndOfGameStats eogStats, LcuQueueStats? rankedStats, Guid profileId, LcuGame? gameDetails = null)
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
        var position = MapPosition(sourcePlayer.Stats.Position, sourcePlayer.Stats.Lane, sourcePlayer.Stats.Role, sourcePlayer.Stats.PlayerPosition);
        
        // Fallback for position if all fields were still empty or "NONE" after augmentation
        if (IsNoneOrEmpty(sourcePlayer.Stats.Position) && 
            IsNoneOrEmpty(sourcePlayer.Stats.Lane) && 
            IsNoneOrEmpty(sourcePlayer.Stats.Role))
        {
            // Guess position based on team index (Top, Jungle, Mid, Bot, Support)
            var team = eogStats.Teams.FirstOrDefault(t => t.Players.Any(p => p.TeamId == sourcePlayer.TeamId));
            if (team != null)
            {
                var index = team.Players.FindIndex(p => p.ChampionId == sourcePlayer.ChampionId);
                position = index switch
                {
                    0 => "Top",
                    1 => "Jungle",
                    2 => "Mid",
                    3 => "ADC",
                    4 => "Support",
                    _ => position
                };
                Console.WriteLine($"[DataMapper] Guessing position by index {index}: {position}");
            }
        }
        
        // Special case: If mapped as Support but champion is a known ADC (like Vayne), and we have "NONE" signals
        if (position == "Support" && IsCommonAdc(sourcePlayer.ChampionId) && 
            (IsNoneOrEmpty(sourcePlayer.Stats.Position) || IsNoneOrEmpty(sourcePlayer.Stats.Lane)))
        {
            Console.WriteLine($"[DataMapper] Correcting position for {championName}: Support -> ADC based on champion type");
            position = "ADC";
        }

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
        
        var match = new MatchEntry
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Champion = championName,
            Role = isSummonersRift ? position : "N/A",
            LaneAlly = laneAlly,
            LaneEnemy = laneEnemy,
            LaneEnemyAlly = laneEnemyAlly,
            Kills = sourcePlayer.Stats.Kills,
            Deaths = sourcePlayer.Stats.Deaths,
            Assists = sourcePlayer.Stats.Assists,
            Cs = csOrVisionScore,
            GameLengthMinutes = gameLengthMinutes,
            Win = isWin,
            Date = DateTime.Now,
            CurrentTier = rankedStats?.Tier ?? "Unranked",
            CurrentDivision = ParseDivision(rankedStats?.Division),
            CurrentLp = rankedStats?.LeaguePoints ?? 0,
            GameMode = gameMode,
            QueueId = eogStats.QueueId
        };
        
        return match;
    }

    public static string MapQueueIdToGameMode(int queueId) => queueId switch
    {
        420 => "Ranked Solo",
        440 => "Ranked Flex",
        400 or 430 => "Normal",
        450 => "ARAM",
        4200 => "ARAM Mayhem",
        900 => "ARURF",
        1900 => "URF",
        1700 => "Arena",
        _ => "Other"
    };
    
    private static string GetChampionName(int championId)
    {
        return ChampionIdToName.TryGetValue(championId, out var name) ? name : $"Champion_{championId}";
    }
    
    private static string MapPosition(string pos, string lane, string role, string playerPos)
    {
        string[] candidates = { pos, lane, role, playerPos };
        foreach (var c in candidates)
        {
            if (string.IsNullOrWhiteSpace(c) || IsNone(c)) continue;
            
            var upper = c.ToUpperInvariant();
            if (upper == "TOP") return "Top";
            if (upper == "JUNGLE") return "Jungle";
            if (upper == "MIDDLE" || upper == "MID") return "Mid";
            if (upper == "BOTTOM" || upper == "BOT" || upper == "ADC" || upper == "CARRY") return "ADC";
            if (upper == "UTILITY" || upper == "SUPPORT") return "Support";
        }
        
        return "ADC"; // Default fallback
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
        
        if (allyTeam == null || enemyTeam == null || allyTeam.Players.Count != 5 || enemyTeam.Players.Count != 5)
        {
            Console.WriteLine("[DataMapper] Could not find complete teams");
            return (laneAlly, laneEnemy, laneEnemyAlly);
        }
        
        // Role index mapping: 0=Top, 1=Jungle, 2=Mid, 3=ADC, 4=Support
        var (allyRole, enemyRole, enemyAllyRole) = position switch
        {
            "Top" => (1, 0, 1),       // Ally: Jungler(1), Enemy: Top(0), EnemyAlly: Jungler(1)
            "Jungle" => (2, 1, 2),    // Ally: Mid(2), Enemy: Jungle(1), EnemyAlly: Mid(2)
            "Mid" => (1, 2, 1),       // Ally: Jungler(1), Enemy: Mid(2), EnemyAlly: Jungler(1)
            "ADC" => (4, 3, 4),       // Ally: Support(4), Enemy: ADC(3), EnemyAlly: Support(4)
            "Support" => (3, 4, 3),   // Ally: ADC(3), Enemy: Support(4), EnemyAlly: ADC(3)
            _ => (-1, -1, -1)
        };
        
        if (allyRole < 0)
        {
            Console.WriteLine($"[DataMapper] Unknown position: {position}");
            return (laneAlly, laneEnemy, laneEnemyAlly);
        }
        
        // Try to find by position fields first, fallback to index
        var allyPlayer = FindPlayerByRole(allyTeam.Players, GetRoleStringForIndex(allyRole), localPlayer.ChampionId) 
                         ?? (allyRole < allyTeam.Players.Count ? allyTeam.Players[allyRole] : null);
        var enemyPlayer = FindPlayerByRole(enemyTeam.Players, GetRoleStringForIndex(enemyRole), -1) 
                          ?? (enemyRole < enemyTeam.Players.Count ? enemyTeam.Players[enemyRole] : null);
        var enemyAllyPlayer = FindPlayerByRole(enemyTeam.Players, GetRoleStringForIndex(enemyAllyRole), -1)
                              ?? (enemyAllyRole < enemyTeam.Players.Count ? enemyTeam.Players[enemyAllyRole] : null);
        
        if (allyPlayer != null && allyPlayer.ChampionId != localPlayer.ChampionId)
            laneAlly = GetChampionName(allyPlayer.ChampionId);
        if (enemyPlayer != null)
            laneEnemy = GetChampionName(enemyPlayer.ChampionId);
        if (enemyAllyPlayer != null && enemyAllyPlayer.ChampionId != enemyPlayer?.ChampionId)
            laneEnemyAlly = GetChampionName(enemyAllyPlayer.ChampionId);
        
        Console.WriteLine($"[DataMapper] Result - LaneAlly: '{laneAlly}', LaneEnemy: '{laneEnemy}', LaneEnemyAlly: '{laneEnemyAlly}'");
        
        return (laneAlly, laneEnemy, laneEnemyAlly);
    }
    
    private static string GetRoleStringForIndex(int index) => index switch
    {
        0 => "TOP",
        1 => "JUNGLE",
        2 => "MIDDLE",
        3 => "BOTTOM",
        4 => "UTILITY",
        _ => ""
    };
    
    private static LcuPlayer? FindPlayerByRole(List<LcuPlayer> players, string targetRole, int excludeChampionId)
    {
        return players.FirstOrDefault(p => 
            p.ChampionId != excludeChampionId &&
            (MatchesRole(p.Stats.Position, targetRole) || 
             MatchesRole(p.Stats.Lane, targetRole) || 
             MatchesRole(p.Stats.Role, targetRole)));
    }
    
    private static bool MatchesRole(string playerRole, string targetRole)
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
    
    private static bool IsSupport(string position)
    {
        if (string.IsNullOrWhiteSpace(position))
            return false;
        
        var pos = position.ToUpperInvariant();
        return pos == "UTILITY" || pos == "SUPPORT";
    }
    
    private static bool IsBottomLane(string position)
    {
        if (string.IsNullOrWhiteSpace(position))
            return false;
        
        var pos = position.ToUpperInvariant();
        return pos == "BOTTOM" || pos == "BOT" || pos == "ADC";
    }
}
