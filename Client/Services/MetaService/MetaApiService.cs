using LolStatsTracker.Services.Common;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.MetaService;

public class MetaApiService : BaseApiService, IMetaService
{
    public MetaApiService(HttpClient http, UserProfileState userState) 
        : base(http, userState) { }

    public async Task<MetaComparisonSummaryDto?> GetComparisonAsync()
    {
        return await GetAsync<MetaComparisonSummaryDto>("api/Meta/comparison");
    }
}
