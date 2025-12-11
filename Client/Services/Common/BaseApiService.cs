using System.Net.Http.Json;
using LolStatsTracker.Services.UserState;

namespace LolStatsTracker.Services.Common;

public abstract class BaseApiService
{
    protected readonly HttpClient Http;
    protected readonly UserProfileState UserState;

    protected BaseApiService(HttpClient http, UserProfileState userState)
    {
        Http = http;
        UserState = userState;
    }

    protected bool HasProfile => UserState.CurrentProfile != null;

    protected HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        if (UserState.CurrentProfile != null)
        {
            request.Headers.Add("X-Profile-Id", UserState.CurrentProfile.Id.ToString());
        }
        return request;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        if (!HasProfile) return default;

        try
        {
            var request = CreateRequest(HttpMethod.Get, url);
            var response = await Http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            return default;
        }
        catch (HttpRequestException)
        {
            return default;
        }
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        if (!HasProfile) throw new InvalidOperationException("No profile selected");

        var request = CreateRequest(HttpMethod.Post, url);
        request.Content = JsonContent.Create(body);
        
        var response = await Http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body)
    {
        if (!HasProfile) return default;

        var request = CreateRequest(HttpMethod.Put, url);
        request.Content = JsonContent.Create(body);
        
        var response = await Http.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        return default;
    }

    protected async Task<bool> DeleteAsync(string url)
    {
        if (!HasProfile) return false;

        var request = CreateRequest(HttpMethod.Delete, url);
        var response = await Http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}
