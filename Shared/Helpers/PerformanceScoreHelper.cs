namespace LolStatsTracker.Shared.Helpers;

public static class PerformanceScoreHelper
{
    /// <summary>
    /// Formula: KDA Score (40%) + CS/min or VisionScore Score (30%) + Win Bonus (30%)
    /// For Support: Uses VisionScore instead of CS with different scaling.
    /// </summary>
    public static int Calculate(int kills, int deaths, int assists, int csOrVisionScore, int gameLengthMinutes, bool win, string role = "ADC")
    {
        if (gameLengthMinutes <= 0) gameLengthMinutes = 1;
        
        var kda = (kills + assists) / Math.Max(1.0, deaths);
        var kdaScore = Math.Min(40, kda * 4);
        
        double resourceScore;
        if (string.Equals(role, "Support", StringComparison.OrdinalIgnoreCase))
        {
            // For Support: VisionScore scoring
            // Good vision score is ~1.5 per minute, excellent is 2+
            var visionPerMin = (double)csOrVisionScore / gameLengthMinutes;
            resourceScore = Math.Min(30, visionPerMin * 15); // 2 vision/min = 30 points
        }
        else
        {
            // For other roles: CS scoring
            var csPerMin = (double)csOrVisionScore / gameLengthMinutes;
            resourceScore = Math.Min(30, csPerMin * 3); // 10 cs/min = 30 points
        }
        
        var winBonus = win ? 30 : 0;
        
        var totalScore = kdaScore + resourceScore + winBonus;
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
