namespace LolStatsTracker.Shared.Constants;

public static class MetaTierList
{
    public static readonly IReadOnlyList<MetaTierEntry> AdcTiers = new List<MetaTierEntry>
    {
        // S-Tier (Optimal picks)
        new("Jinx", "S"),
        new("Kai'Sa", "S"),
        new("Jhin", "S"),
        new("Caitlyn", "S"),
        
        // A-Tier (Strong picks)
        new("Vayne", "A"),
        new("Miss Fortune", "A"),
        new("Draven", "A"),
        new("Ezreal", "A"),
        new("Ashe", "A"),
        
        // B-Tier (Viable picks)
        new("Samira", "B"),
        new("Lucian", "B"),
        new("Tristana", "B"),
        new("Xayah", "B"),
        new("Twitch", "B"),
        
        // C-Tier (Situational picks)
        new("Sivir", "C"),
        new("Kog'Maw", "C"),
        new("Kalista", "C"),
        new("Aphelios", "C"),
        
        // D-Tier (Weak picks)
        new("Zeri", "D"),
        new("Nilah", "D"),
        new("Varus", "D")
    };

    public static readonly IReadOnlyDictionary<string, int> TierPriority = new Dictionary<string, int>
    {
        ["S"] = 1,
        ["A"] = 2,
        ["B"] = 3,
        ["C"] = 4,
        ["D"] = 5
    };

    public static IEnumerable<string> GetChampionsInTier(string tier) =>
        AdcTiers.Where(t => t.Tier == tier).Select(t => t.Champion);

    public static bool IsMetaChampion(string champion) =>
        AdcTiers.Any(t => t.Champion.Equals(champion, StringComparison.OrdinalIgnoreCase));

    public static string? GetChampionTier(string champion) =>
        AdcTiers.FirstOrDefault(t => t.Champion.Equals(champion, StringComparison.OrdinalIgnoreCase))?.Tier;
}

public record MetaTierEntry(string Champion, string Tier);
