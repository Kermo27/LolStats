using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Services.MatchService;

public interface IMatchService
{
    Task<List<MatchEntry>> GetAllAsync();
    Task<List<MatchEntry>> GetRecentAsync(int count, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<PaginatedResponse<MatchEntry>> GetPaginatedAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null);
    Task<MatchEntry?> GetAsync(Guid id);
    Task<MatchEntry> AddAsync(MatchEntry match);
    Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match);
    Task DeleteAsync(Guid id);
    Task ClearAsync();
}