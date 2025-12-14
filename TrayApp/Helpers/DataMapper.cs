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
    
    public static MatchEntry MapToMatchEntry(LcuEndOfGameStats eogStats, LcuQueueStats? rankedStats, Guid profileId)
    {
        var localPlayer = eogStats.LocalPlayer;
        if (localPlayer == null)
            throw new ArgumentException("Local player data is missing");
        
        var championName = GetChampionName(localPlayer.ChampionId);
        var position = MapPosition(localPlayer.Stats.Position);
        var totalCs = localPlayer.Stats.MinionsKilled + localPlayer.Stats.NeutralMinionsKilled;
        var gameLengthMinutes = eogStats.GameLength / 60;
        
        // Find team info
        var playerTeam = eogStats.Teams.FirstOrDefault(t => 
            t.Players.Any(p => p.SummonerName == localPlayer.SummonerName));
        
        var isWin = playerTeam?.IsWinningTeam ?? localPlayer.Stats.Win;
        
        // Detect support, enemy bot, enemy support (simplified - needs team composition analysis)
        var (support, enemyBot, enemySupport) = DetectLaneOpponents(eogStats, localPlayer, position);
        
        var match = new MatchEntry
        {
            Id = Guid.NewGuid(),
            GameId = eogStats.GameId,
            ProfileId = profileId,
            Champion = championName,
            Role = position,
            Support = support,
            EnemyBot = enemyBot,
            EnemySupport = enemySupport,
            Kills = localPlayer.Stats.Kills,
            Deaths = localPlayer.Stats.Deaths,
            Assists = localPlayer.Stats.Assists,
            Cs = totalCs,
            GameLengthMinutes = gameLengthMinutes,
            Win = isWin,
            Date = DateTime.Now,
            CurrentTier = rankedStats?.Tier ?? "Unranked",
            CurrentDivision = ParseDivision(rankedStats?.Division),
            CurrentLp = rankedStats?.LeaguePoints ?? 0,
            LpChange = 0 // Can't get LP change from LCU easily
        };
        
        return match;
    }
    
    private static string GetChampionName(int championId)
    {
        return ChampionIdToName.TryGetValue(championId, out var name) ? name : $"Champion_{championId}";
    }
    
    private static string MapPosition(string lcuPosition)
    {
        return lcuPosition.ToUpper() switch
        {
            "TOP" => "Top",
            "JUNGLE" => "Jungle",
            "MIDDLE" or "MID" => "Mid",
            "BOTTOM" or "BOT" => "ADC",
            "UTILITY" or "SUPPORT" => "Support",
            _ => "ADC"
        };
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
    
    private static (string support, string enemyBot, string enemySupport) DetectLaneOpponents(
        LcuEndOfGameStats eogStats, LcuPlayer localPlayer, string position)
    {
        var support = "";
        var enemyBot = "";
        var enemySupport = "";
        
        if (position != "ADC")
            return (support, enemyBot, enemySupport);
        
        // Find allied support (same team, UTILITY position)
        var allyTeam = eogStats.Teams.FirstOrDefault(t => 
            t.Players.Any(p => p.SummonerName == localPlayer.SummonerName));
        
        var allySupport = allyTeam?.Players.FirstOrDefault(p => 
            p.Stats.Position.ToUpper() == "UTILITY" || p.Stats.Position.ToUpper() == "SUPPORT");
        
        if (allySupport != null)
            support = GetChampionName(allySupport.ChampionId);
        
        // Find enemy bot lane (opposite team, BOTTOM + UTILITY)
        var enemyTeam = eogStats.Teams.FirstOrDefault(t => 
            !t.Players.Any(p => p.SummonerName == localPlayer.SummonerName));
        
        var enemyAdc = enemyTeam?.Players.FirstOrDefault(p => 
            p.Stats.Position.ToUpper() == "BOTTOM" || p.Stats.Position.ToUpper() == "BOT");
        var enemySup = enemyTeam?.Players.FirstOrDefault(p => 
            p.Stats.Position.ToUpper() == "UTILITY" || p.Stats.Position.ToUpper() == "SUPPORT");
        
        if (enemyAdc != null)
            enemyBot = GetChampionName(enemyAdc.ChampionId);
        if (enemySup != null)
            enemySupport = GetChampionName(enemySup.ChampionId);
        
        return (support, enemyBot, enemySupport);
    }
}
