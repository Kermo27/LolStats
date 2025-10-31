using System.Text.Json;
using Microsoft.JSInterop;

namespace LolStatsTracker.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;
    
    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SaveAsync<T>(string key, T data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);   
    }

    public async Task<T?> LoadAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json))
            return default;
        
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);   
    }
}