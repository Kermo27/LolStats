namespace LolStatsTracker.Shared.Helpers;

public static class RankHelper
{
    private static readonly string[] Tiers = 
    { 
        "Iron", "Bronze", "Silver", "Gold", "Platinum", "Emerald", "Diamond", "Master", "Grandmaster", "Challenger" 
    };
    
    private const int LpPerDivision = 100;
    private const int DivisionsPerTier = 4; // IV, III, II, I
    private const int LpPerTier = LpPerDivision * DivisionsPerTier; // 400 LP per tier

    public static int CalculateTotalLp(string tier, int division, int currentLp)
    {
        var tierIndex = GetTierIndex(tier);
        if (tierIndex < 0) return currentLp; // Unknown tier, just return LP
        
        // Master+ tiers don't have divisions (division = 1, but LP can go much higher)
        if (tierIndex >= 7) // Master, Grandmaster, Challenger
        {
            // Master starts at 2400 (7 * 400), GM at 2800, Challenger at 3200
            // In these tiers, LP is unbounded
            return (tierIndex * LpPerTier) + currentLp;
        }
        
        var divisionOffset = (DivisionsPerTier - division) * LpPerDivision;
        
        return (tierIndex * LpPerTier) + divisionOffset + currentLp;
    }

    public static int GetTierIndex(string tier)
    {
        for (int i = 0; i < Tiers.Length; i++)
        {
            if (string.Equals(Tiers[i], tier, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1; // Unknown tier
    }
    
    public static string FormatRank(string tier, int division, int lp)
    {
        var tierIndex = GetTierIndex(tier);
        
        if (tierIndex >= 7)
            return $"{tier} {lp} LP";
        
        var divisionRoman = division switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            _ => ""
        };
        
        return $"{tier} {divisionRoman} ({lp} LP)";
    }
}
