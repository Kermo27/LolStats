using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.Constants;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.MetaService;

public class MetaService : IMetaService
{
    private readonly MatchDbContext _db;

    public MetaService(MatchDbContext db) => _db = db;

    public IEnumerable<object> GetTiers()
    {
        return MetaTierList.AdcTiers.Select(t => new { t.Champion, t.Tier });
    }

    public async Task<MetaComparisonSummaryDto> GetComparisonAsync(Guid profileId)
    {
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .ToListAsync();

        var championStats = ChampionStatsHelper.GroupByChampion(matches);

        var metaChampions = MetaTierList.AdcTiers.Select(t =>
        {
            var isPlayed = championStats.ContainsKey(t.Champion);
            var stats = isPlayed ? championStats[t.Champion] : new ChampionStatsResult(0, 0, 0, 0, 0);
            
            return new MetaChampionDto(
                t.Champion,
                t.Tier,
                "ADC",
                isPlayed,
                stats.Games,
                stats.Winrate
            );
        }).ToList();

        var metaPlayed = metaChampions.Count(c => c.IsPlayed && (c.Tier == "S" || c.Tier == "A"));
        var offMeta = championStats.Keys.Count(c => !MetaTierList.IsMetaChampion(c));

        var recommendation = GenerateRecommendation(metaPlayed);

        return new MetaComparisonSummaryDto(metaChampions, metaPlayed, offMeta, recommendation);
    }

    private static string GenerateRecommendation(int metaPlayed) => metaPlayed switch
    {
        >= 3 => "Great! You're playing meta picks.",
        >= 1 => "Consider adding more S/A tier champions to your pool.",
        _ => "Your pool lacks meta champions. Consider learning Jinx, Kai'Sa or Jhin."
    };
}
