using LolStatsTracker.Services.Common;
using LolStatsTracker.Services.UserState;
using LolStatsTracker.Shared.Models;

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