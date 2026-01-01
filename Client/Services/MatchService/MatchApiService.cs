using LolStatsTracker.Services.Common;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using System.Web;

namespace LolStatsTracker.Services.MatchService;

public class MatchApiService : BaseApiService, IMatchService
{
    private const string BaseUrl = "api/Matches";

    public MatchApiService(HttpClient http, IUserProfileState userState) 
        : base(http, userState) { }

    public async Task<List<MatchEntry>> GetAllAsync()
    {
        return await GetAsync<List<MatchEntry>>(BaseUrl) ?? new List<MatchEntry>();
    }
    
    public async Task<List<MatchEntry>> GetRecentAsync(int count, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["count"] = count.ToString();
        if (startDate.HasValue) queryParams["startDate"] = startDate.Value.ToString("o");
        if (endDate.HasValue) queryParams["endDate"] = endDate.Value.ToString("o");
        if (!string.IsNullOrEmpty(gameMode)) queryParams["gameMode"] = gameMode;
        
        var url = $"{BaseUrl}/recent?{queryParams}";
        return await GetAsync<List<MatchEntry>>(url) ?? new List<MatchEntry>();
    }
    
    public async Task<PaginatedResponse<MatchEntry>> GetPaginatedAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, string? gameMode = null)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["page"] = page.ToString();
        queryParams["pageSize"] = pageSize.ToString();
        if (startDate.HasValue) queryParams["startDate"] = startDate.Value.ToString("o");
        if (endDate.HasValue) queryParams["endDate"] = endDate.Value.ToString("o");
        if (!string.IsNullOrEmpty(gameMode)) queryParams["gameMode"] = gameMode;
        
        var url = $"{BaseUrl}/paginated?{queryParams}";
        return await GetAsync<PaginatedResponse<MatchEntry>>(url) ?? new PaginatedResponse<MatchEntry>(new List<MatchEntry>(), 0, page, pageSize);
    }
    
    public async Task<MatchEntry?> GetAsync(Guid id)
    {
        return await GetAsync<MatchEntry>($"{BaseUrl}/{id}");
    }

    public async Task<MatchEntry> AddAsync(MatchEntry match)
    {
        return await PostAsync<MatchEntry, MatchEntry>(BaseUrl, match) ?? match;
    }

    public async Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match)
    {
        return await PutAsync<MatchEntry, MatchEntry>($"{BaseUrl}/{id}", match) ?? match;
    }

    public async Task DeleteAsync(Guid id)
    {
        await base.DeleteAsync($"{BaseUrl}/{id}");
    }

    public async Task ClearAsync()
    {
        await base.DeleteAsync($"{BaseUrl}/clear");
    }
}