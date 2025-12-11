using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Shared.Helpers;

public static class ChampionStatsHelper
{
    public static double CalculateKda(int kills, int deaths, int assists)
    {
        if (deaths == 0) return kills + assists;
        return (double)(kills + assists) / deaths;
    }

    public static double CalculateWinrate(int wins, int totalGames)
    {
        if (totalGames == 0) return 0;
        return (double)wins / totalGames;
    }

    public static ChampionStatsResult CalculateStats(IEnumerable<MatchEntry> matches)
    {
        var matchList = matches.ToList();
        
        if (matchList.Count == 0)
        {
            return new ChampionStatsResult(0, 0, 0, 0, 0);
        }

        var games = matchList.Count;
        var wins = matchList.Count(m => m.Win);
        var winrate = CalculateWinrate(wins, games);
        var avgKda = matchList.Average(m => CalculateKda(m.Kills, m.Deaths, m.Assists));
        var avgCs = matchList.Average(m => m.GameLengthMinutes > 0 ? (double)m.Cs / m.GameLengthMinutes : 0);

        return new ChampionStatsResult(games, wins, winrate, Math.Round(avgKda, 2), Math.Round(avgCs, 1));
    }

    public static Dictionary<string, ChampionStatsResult> GroupByChampion(IEnumerable<MatchEntry> matches)
    {
        return matches
            .GroupBy(m => m.Champion)
            .ToDictionary(g => g.Key, g => CalculateStats(g));
    }
}

public record ChampionStatsResult(
    int Games,
    int Wins,
    double Winrate,
    double AvgKda,
    double AvgCsPerMin
);
