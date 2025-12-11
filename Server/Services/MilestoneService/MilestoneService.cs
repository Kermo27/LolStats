using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.Constants;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.MilestoneService;

public class MilestoneService : IMilestoneService
{
    private readonly MatchDbContext _db;

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

        var comparison = RankConstants.CompareRanks(newTier, newDiv, prevTier, prevDiv);
        if (comparison == 0) return;

        var type = comparison > 0 ? "Promotion" : "Demotion";

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
}
