using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.MilestoneService;

public interface IMilestoneService
{
    Task<List<RankMilestoneDto>> GetMilestonesAsync();
}
