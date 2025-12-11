using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.MilestoneService;

public interface IMilestoneService
{
    Task<List<RankMilestoneDto>> GetMilestonesAsync(Guid profileId);
    Task<RankMilestone> CreateAsync(Guid profileId, RankMilestoneCreateDto dto);
    Task CheckAndRecordMilestoneAsync(Guid profileId, MatchEntry match, MatchEntry? previousMatch);
}
