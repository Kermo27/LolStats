using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.MilestoneService;

public class MilestoneService : IMilestoneService
{
    private readonly MatchDbContext _db;
    
    private static readonly Dictionary<string, int> TierOrder = new()
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

    public MilestoneService(MatchDbContext db) => _db = db;

    public async Task<List<RankMilestoneDto>> GetMilestonesAsync(Guid profileId)
    {
        return await _db.RankMilestones
            .Where(m => m.ProfileId == profileId)
            .OrderByDescending(m => m.AchievedAt)
            .Select(m => new RankMilestoneDto(m.Id, m.Tier, m.Division, m.AchievedAt, m.Type, m.MatchId))
            .ToListAsync();
    }

    public async Task<RankMilestone> CreateAsync(Guid profileId, RankMilestoneCreateDto dto)
    {
        var milestone = new RankMilestone
        {
            ProfileId = profileId,
            Tier = dto.Tier,
            Division = dto.Division,
            AchievedAt = dto.AchievedAt ?? DateTime.UtcNow,
            Type = "Manual"
        };

        _db.RankMilestones.Add(milestone);
        await _db.SaveChangesAsync();
        return milestone;
    }

    public async Task CheckAndRecordMilestoneAsync(Guid profileId, MatchEntry match, MatchEntry? previousMatch)
    {
        if (previousMatch == null) return;

        var (prevTier, prevDiv) = (previousMatch.CurrentTier, previousMatch.CurrentDivision);
        var (newTier, newDiv) = (match.CurrentTier, match.CurrentDivision);

        if (prevTier == newTier && prevDiv == newDiv) return;

        var prevRank = GetRankValue(prevTier, prevDiv);
        var newRank = GetRankValue(newTier, newDiv);

        if (prevRank == newRank) return;

        var type = newRank > prevRank ? "Promotion" : "Demotion";

        var milestone = new RankMilestone
        {
            ProfileId = profileId,
            Tier = newTier,
            Division = newDiv,
            AchievedAt = match.Date,
            MatchId = match.Id,
            Type = type
        };

        _db.RankMilestones.Add(milestone);
        await _db.SaveChangesAsync();
    }

    private static int GetRankValue(string tier, int division)
    {
        if (!TierOrder.TryGetValue(tier, out var tierValue)) return 0;
        // Higher tier = higher value, Lower division (1) = higher rank
        // Master+ have no divisions, treat as division 0
        if (tierValue >= 8) return tierValue * 10;
        return tierValue * 10 + (4 - division);
    }
}
