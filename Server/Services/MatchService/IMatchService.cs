using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Services.MatchService;

public interface IMatchService
{
    Task<List<MatchEntry>> GetAllAsync(Guid profileId);
    Task<List<MatchEntry>> GetRecentAsync(Guid profileId, int count, DateTime? startDate, DateTime? endDate, string? gameMode);
    Task<PaginatedResponse<MatchEntry>> GetPaginatedAsync(Guid profileId, int page, int pageSize, DateTime? startDate, DateTime? endDate, string? gameMode);
    Task<MatchEntry?> GetAsync(Guid id, Guid profileId);
    Task<MatchEntry> AddAsync(MatchEntry match);
    Task<MatchEntry?> UpdateAsync(Guid id, MatchEntry match, Guid profileId);
    Task<bool> DeleteAsync(Guid id);
    Task ClearAsync(Guid profileId);
}