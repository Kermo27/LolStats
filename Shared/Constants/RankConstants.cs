namespace LolStatsTracker.Shared.Constants;

public static class RankConstants
{
    public static readonly IReadOnlyList<string> Tiers = new[]
    {
        "Iron", "Bronze", "Silver", "Gold", "Platinum", "Emerald", "Diamond", "Master", "Grandmaster", "Challenger"
    };

    public static readonly IReadOnlyDictionary<string, int> TierOrder = new Dictionary<string, int>
    {
        ["Iron"] = 1,
        ["Bronze"] = 2,
        ["Silver"] = 3,
        ["Gold"] = 4,
        ["Platinum"] = 5,
        ["Emerald"] = 6,
        ["Diamond"] = 7,
        ["Master"] = 8,
        ["Grandmaster"] = 9,
        ["Challenger"] = 10
    };

    public static readonly IReadOnlySet<string> ApexTiers = new HashSet<string>
    {
        "Master", "Grandmaster", "Challenger"
    };

    public static int GetRankValue(string tier, int division)
    {
        if (!TierOrder.TryGetValue(tier, out var tierValue)) return 0;
        
        if (ApexTiers.Contains(tier)) return tierValue * 10;
        
        return tierValue * 10 + (4 - division);
    }
    
    public static string GetDivisionRoman(int division) => division switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        4 => "IV",
        _ => ""
    };

    public static int CompareRanks(string tier1, int division1, string tier2, int division2)
    {
        return GetRankValue(tier1, division1) - GetRankValue(tier2, division2);
    }
}
