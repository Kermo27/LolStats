using System.Net.Http.Json;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.AdminService;

public class AdminApiService : IAdminService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/Admin";

    public AdminApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<SystemStatsDto?> GetStatsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<SystemStatsDto>($"{BaseUrl}/stats");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<UserListDto>> GetUsersAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<UserListDto>>($"{BaseUrl}/users") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, string role)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"{BaseUrl}/users/{userId}/role", new UpdateUserRoleDto(role));
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/users/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PaginatedResponse<AdminMatchDto>?> GetMatchesAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        try
        {
            var url = $"{BaseUrl}/matches?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }
            return await _http.GetFromJsonAsync<PaginatedResponse<AdminMatchDto>>(url);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteMatchAsync(Guid matchId)
    {
        try
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/matches/{matchId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<ProfileListDto>> GetProfilesAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ProfileListDto>>($"{BaseUrl}/profiles") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<ProfileListDto>> GetProfilesByUserIdAsync(Guid userId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ProfileListDto>>($"{BaseUrl}/users/{userId}/profiles") ?? new();
        }
        catch
        {
            return new();
        }
    }
}
