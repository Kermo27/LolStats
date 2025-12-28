using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.TrayApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LolStatsTracker.TrayApp.Services;

/// <summary>
/// Service for handling authentication in TrayApp with secure token storage
/// </summary>
public class TrayAuthService
{
    private readonly ILogger<TrayAuthService> _logger;
    private readonly HttpClient _httpClient;
    private readonly AppConfiguration _config;
    private readonly string _tokenFilePath;
    
    private TokenResponseDto? _currentToken;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
    
    public bool IsAuthenticated => _currentToken != null && DateTime.UtcNow < _currentToken.ExpiresAt;
    public string? AccessToken => _currentToken?.AccessToken;
    public UserInfoDto? CurrentUser => _currentToken?.User;

    public async Task<string?> GetValidAccessTokenAsync()
    {
        if (_currentToken == null) return null;
        
        // Refresh if expired or expiring within 5 minutes
        if (DateTime.UtcNow.AddMinutes(5) >= _currentToken.ExpiresAt)
        {
            _logger.LogInformation("Token expiring soon, attempting refresh...");
            var success = await RefreshTokenAsync();
            if (!success) return null;
        }
        
        return _currentToken?.AccessToken;
    }

    public TrayAuthService(
        ILogger<TrayAuthService> logger,
        IOptions<AppConfiguration> config,
        HttpClient httpClient)
    {
        _logger = logger;
        _config = config.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_config.ApiBaseUrl);
        
        // Store token in AppData
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "LolStatsTracker");
        Directory.CreateDirectory(appFolder);
        _tokenFilePath = Path.Combine(appFolder, ".auth");
    }

    public async Task<bool> TryLoadStoredTokenAsync()
    {
        try
        {
            if (!File.Exists(_tokenFilePath))
                return false;

            var encryptedData = await File.ReadAllBytesAsync(_tokenFilePath);
            var decryptedJson = Unprotect(encryptedData);
            
            _currentToken = JsonSerializer.Deserialize<TokenResponseDto>(decryptedJson);
            
            if (_currentToken == null)
                return false;

            // Check if token is expired
            if (DateTime.UtcNow >= _currentToken.ExpiresAt)
            {
                // Try to refresh
                var refreshed = await RefreshTokenAsync();
                return refreshed;
            }

            _logger.LogInformation("Loaded stored token for user: {Username}", _currentToken.User.Username);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load stored token");
            await ClearStoredTokenAsync();
            return false;
        }
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginDto
            {
                Username = username,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                if (token != null)
                {
                    _currentToken = token;
                    await StoreTokenAsync(token);
                    _logger.LogInformation("User logged in: {Username}", token.User.Username);
                    return (true, null);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string password, string? email = null)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new RegisterDto
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
                    _currentToken = token;
                    await StoreTokenAsync(token);
                    _logger.LogInformation("User registered: {Username}", token.User.Username);
                    return (true, null);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return (false, ex.Message);
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            if (_currentToken == null) return false;
            
            // Re-check if token was refreshed while waiting for semaphore
            if (DateTime.UtcNow.AddMinutes(5) < _currentToken.ExpiresAt)
            {
                _logger.LogInformation("Token was already refreshed by another thread");
                return true;
            }

            if (string.IsNullOrEmpty(_currentToken.RefreshToken))
            {
                _logger.LogWarning("Cannot refresh: no refresh token available");
                return false;
            }

            _logger.LogInformation("Refreshing token for user: {Username}", _currentToken.User.Username);

            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenDto
            {
                RefreshToken = _currentToken.RefreshToken
            });

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                if (token != null)
                {
                    _currentToken = token;
                    await StoreTokenAsync(token);
                    _logger.LogInformation("Token refreshed successfully");
                    return true;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Token refresh failed. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            
            await ClearStoredTokenAsync();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during token refresh");
            await ClearStoredTokenAsync();
            return false;
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
            if (_currentToken != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _currentToken.AccessToken);
                await _httpClient.PostAsync("api/auth/logout", null);
            }
        }
        catch
        {
            // Ignore errors during logout
        }
        finally
        {
            _currentToken = null;
            await ClearStoredTokenAsync();
            _logger.LogInformation("User logged out");
        }
    }

    public void AddAuthHeader(HttpRequestMessage request)
    {
        if (_currentToken != null)
        {
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", _currentToken.AccessToken);
        }
    }

    private async Task StoreTokenAsync(TokenResponseDto token)
    {
        try
        {
            var json = JsonSerializer.Serialize(token);
            var encryptedData = Protect(json);
            await File.WriteAllBytesAsync(_tokenFilePath, encryptedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store token");
        }
    }

    private async Task ClearStoredTokenAsync()
    {
        try
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
            }
            _currentToken = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear stored token");
        }
        await Task.CompletedTask;
    }

    // Simple data protection using DPAPI (Windows only)
    private static byte[] Protect(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
    }

    private static string Unprotect(byte[] data)
    {
        var bytes = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }
}
