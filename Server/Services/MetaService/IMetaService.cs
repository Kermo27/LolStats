using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.API.Services.MetaService;

public interface IMetaService
{
    Task<MetaComparisonSummaryDto> GetComparisonAsync(Guid profileId);
    IEnumerable<object> GetTiers();
}
