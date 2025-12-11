namespace LolStatsTracker.Shared.Helpers;

public static class PerformanceScoreHelper
{
    // Formula: KDA Score (40%) + CS/min Score (30%) + Win Bonus (30%)
    public static int Calculate(int kills, int deaths, int assists, int cs, int gameLengthMinutes, bool win)
    {
        if (gameLengthMinutes <= 0) gameLengthMinutes = 1;
        
        var kda = (kills + assists) / Math.Max(1.0, deaths);
        var kdaScore = Math.Min(40, kda * 4);
        
        var csPerMin = (double)cs / gameLengthMinutes;
        var csScore = Math.Min(30, csPerMin * 3);
        
        var winBonus = win ? 30 : 0;
        
        var totalScore = kdaScore + csScore + winBonus;
        return (int)Math.Round(Math.Min(100, Math.Max(0, totalScore)));
    }

    public static string GetRating(int score) => score switch
    {
        >= 90 => "S+",
        >= 80 => "S",
        >= 70 => "A",
        >= 60 => "B",
        >= 50 => "C",
        >= 40 => "D",
        _ => "F"
    };
    
    public static string GetScoreColorClass(int score) => score switch
    {
        >= 80 => "wr-excellent",
        >= 60 => "wr-good", 
        >= 50 => "wr-average",
        _ => "wr-poor"
    };
}
