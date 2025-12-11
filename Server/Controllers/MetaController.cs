using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetaController : BaseApiController
{
    private readonly MatchDbContext _db;
    
    // Hardcoded ADC tier list (Season 15 example data)
    private static readonly List<(string Champion, string Tier)> AdcMetaTiers = new()
    {
        // S-Tier
        ("Jinx", "S"), ("Kai'Sa", "S"), ("Jhin", "S"), ("Caitlyn", "S"),
        // A-Tier
        ("Vayne", "A"), ("Miss Fortune", "A"), ("Draven", "A"), ("Ezreal", "A"), ("Ashe", "A"),
        // B-Tier
        ("Samira", "B"), ("Lucian", "B"), ("Tristana", "B"), ("Xayah", "B"), ("Twitch", "B"),
        // C-Tier
        ("Sivir", "C"), ("Kog'Maw", "C"), ("Kalista", "C"), ("Aphelios", "C"),
        // D-Tier
        ("Zeri", "D"), ("Nilah", "D"), ("Varus", "D")
    };

    public MetaController(MatchDbContext db) => _db = db;

    [HttpGet("tiers")]
    public ActionResult<List<(string Champion, string Tier)>> GetTiers()
    {
        return Ok(AdcMetaTiers.Select(t => new { t.Champion, t.Tier }));
    }

    [HttpGet("comparison")]
    public async Task<ActionResult<MetaComparisonSummaryDto>> GetComparison()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID missing");

        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .ToListAsync();

        var championStats = matches
            .GroupBy(m => m.Champion)
            .ToDictionary(
                g => g.Key,
                g => (Games: g.Count(), Winrate: g.Any() ? (double)g.Count(m => m.Win) / g.Count() : 0)
            );

        var metaChampions = AdcMetaTiers.Select(t =>
        {
            var isPlayed = championStats.ContainsKey(t.Champion);
            var stats = isPlayed ? championStats[t.Champion] : (Games: 0, Winrate: 0.0);
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
        var offMeta = championStats.Keys.Count(c => !AdcMetaTiers.Any(t => t.Champion == c));

        var recommendation = metaPlayed switch
        {
            >= 3 => "Great! You're playing meta picks.",
            >= 1 => "Consider adding more S/A tier champions to your pool.",
            _ => "Your pool lacks meta champions. Consider learning Jinx, Kai'Sa or Jhin."
        };

        return Ok(new MetaComparisonSummaryDto(metaChampions, metaPlayed, offMeta, recommendation));
    }
}
