using System.Net.Http.Json;
using Blazored.LocalStorage;
using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Services.AuthService;

public interface IAuthService
{
    Task<(bool Success, string? Error, TokenResponseDto? Token)> LoginAsync(string username, string password);
    Task<(bool Success, string? Error, TokenResponseDto? Token)> RegisterAsync(string username, string password, string? email = null);
    Task<(bool Success, TokenResponseDto? Token)> RefreshTokenAsync();
    Task LogoutAsync();
    Task<string?> GetAccessTokenAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<UserInfoDto?> GetCurrentUserAsync();
}

    public class AuthenticationService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
        private const string AccessTokenKey = "accessToken";
        private const string RefreshTokenKey = "refreshToken";
        private const string TokenExpiryKey = "tokenExpiry";
        private const string UserInfoKey = "userInfo";

        public AuthenticationService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

    public async Task<(bool Success, string? Error, TokenResponseDto? Token)> LoginAsync(string username, string password)
    {
        try
        {
            // Clear any existing tokens first
            await ClearTokensAsync();
            
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginDto
            {
                Username = username,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                if (token != null)
                {
                    await StoreTokensAsync(token);
                    return (true, null, token);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string? Error, TokenResponseDto? Token)> RegisterAsync(
        string username, string password, string? email = null)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new RegisterDto
            {
                Username = username,
                Password = password,
                Email = email
            });

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                if (token != null)
                {
                    await StoreTokensAsync(token);
                    return (true, null, token);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, TokenResponseDto? Token)> RefreshTokenAsync()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            // Re-check if token was already refreshed by another concurrent call
            var expiry = await _localStorage.GetItemAsync<DateTime?>(TokenExpiryKey);
            if (expiry.HasValue && expiry.Value.AddMinutes(-5) > DateTime.UtcNow)
            {
                // Token is now valid, another thread refreshed it
                var accessToken = await _localStorage.GetItemAsync<string>(AccessTokenKey);
                var refreshTokenVal = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
                var userInfo = await _localStorage.GetItemAsync<UserInfoDto>(UserInfoKey);
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    return (true, new TokenResponseDto 
                    { 
                        AccessToken = accessToken, 
                        RefreshToken = refreshTokenVal ?? "", 
                        ExpiresAt = expiry.Value,
                        User = userInfo ?? new UserInfoDto()
                    });
                }
            }

            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return (false, null);
            }

            // We use the regular _http here because it handles the /api/auth/refresh skip logic
            var response = await _http.PostAsJsonAsync("api/auth/refresh", new RefreshTokenDto
            {
                RefreshToken = refreshToken
            });

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                if (token != null)
                {
                    await StoreTokensAsync(token);
                    return (true, token);
                }
            }

            // Refresh failed, clear tokens
            await ClearTokensAsync();
            return (false, null);
        }
        catch
        {
            await ClearTokensAsync();
            return (false, null);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                await _http.PostAsync("api/auth/logout", null);
            }
        }
        catch
        {
            // Ignore errors during logout
        }
        finally
        {
            await ClearTokensAsync();
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var expiry = await _localStorage.GetItemAsync<DateTime?>(TokenExpiryKey);
        
        // Check if token is expired (with 5 minute buffer)
        if (expiry.HasValue && expiry.Value.AddMinutes(-5) <= DateTime.UtcNow)
        {
            // Try to refresh
            var (success, token) = await RefreshTokenAsync();
            if (!success)
            {
                return null;
            }
        }

        return await _localStorage.GetItemAsync<string>(AccessTokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetAccessTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<UserInfoDto>(UserInfoKey);
    }

    private async Task StoreTokensAsync(TokenResponseDto token)
    {
        await _localStorage.SetItemAsync(AccessTokenKey, token.AccessToken);
        await _localStorage.SetItemAsync(RefreshTokenKey, token.RefreshToken);
        await _localStorage.SetItemAsync(TokenExpiryKey, token.ExpiresAt);
        await _localStorage.SetItemAsync(UserInfoKey, token.User);
    }

    private async Task ClearTokensAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
        await _localStorage.RemoveItemAsync(TokenExpiryKey);
        await _localStorage.RemoveItemAsync(UserInfoKey);
    }
}
