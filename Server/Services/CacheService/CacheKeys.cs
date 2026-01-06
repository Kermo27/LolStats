namespace LolStatsTracker.API.Services.CacheService;

public static class CacheKeys
{
    // Stats cache keys - per profile, short TTL (30 seconds) since data changes with new matches
    public static string StatsSummary(Guid profileId, int? seasonId, string? gameMode) 
        => $"stats:summary:{profileId}:{seasonId ?? 0}:{gameMode ?? "all"}";
    
    public static string ChampionStats(Guid profileId, int? seasonId, string? gameMode) 
        => $"stats:champions:{profileId}:{seasonId ?? 0}:{gameMode ?? "all"}";
    
    public static string Overview(Guid profileId, int? seasonId, string? gameMode) 
        => $"stats:overview:{profileId}:{seasonId ?? 0}:{gameMode ?? "all"}";

    // DDragon cache keys - longer TTL (24 hours) since this rarely changes
    public static string Champions => "ddragon:champions";
    public static string ChampionImage(string championName) => $"ddragon:champion:image:{championName}";
    public static string LatestVersion => "ddragon:version";

    // User profile cache - medium TTL (5 minutes)
    public static string UserProfile(Guid userId) => $"user:profile:{userId}";
    public static string UserProfiles(Guid userId) => $"user:profiles:{userId}";

    // Invalidation patterns
    public static string ProfilePrefix(Guid profileId) => $"stats:*:{profileId}:*";
}

public static class CacheTTL
{
    public static readonly TimeSpan Short = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Long = TimeSpan.FromHours(1);
    public static readonly TimeSpan DDragon = TimeSpan.FromHours(24);
}
