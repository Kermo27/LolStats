using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.MetaService;

public interface IMetaService
{
    Task<MetaComparisonSummaryDto?> GetComparisonAsync();
}
