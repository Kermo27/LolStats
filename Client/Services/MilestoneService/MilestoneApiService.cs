using LolStatsTracker.Services.Common;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.MilestoneService;

public class MilestoneApiService : BaseApiService, IMilestoneService
{
    private const string BaseUrl = "api/Milestones";

    public MilestoneApiService(HttpClient http, UserProfileState userState) 
        : base(http, userState) { }

    public async Task<List<RankMilestoneDto>> GetMilestonesAsync()
    {
        return await GetAsync<List<RankMilestoneDto>>(BaseUrl) ?? new List<RankMilestoneDto>();
    }
}
