using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace LolStatsTracker.Services;

public class LocalStorageService : IStorageProvider
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SaveAsync<T>(string key, T data)
    {
        var json = JsonSerializer.Serialize(data);
        
        if (json.Length > 10_000)
        {
            var compressed = CompressString(json);
            await _js.InvokeVoidAsync("localStorage.setItem", $"{key}_compressed", compressed);
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
        }
        else
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, json);
        }
    }

    public async Task<T?> LoadAsync<T>(string key)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        var compressed = await _js.InvokeAsync<string?>("localStorage.getItem", $"{key}_compressed");
        
        string? json;
        if (!string.IsNullOrEmpty(compressed))
        {
            json = DecompressString(compressed);
            Console.WriteLine($"Decompressed in {sw.ElapsedMilliseconds}ms");
        }
        else
        {
            json = await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        
        if (string.IsNullOrEmpty(json)) return default;
        
        var result = JsonSerializer.Deserialize<T>(json);
        sw.Stop();
        Console.WriteLine($"LoadAsync {key}: {sw.ElapsedMilliseconds}ms");
        
        return result;
    }

    public async Task RemoveAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
        await _js.InvokeVoidAsync("localStorage.removeItem", $"{key}_compressed");
    }

    private static string CompressString(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            msi.CopyTo(gs);
        }
        return Convert.ToBase64String(mso.ToArray());
    }

    private static string DecompressString(string compressedText)
    {
        var bytes = Convert.FromBase64String(compressedText);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.CopyTo(mso);
        }
        return Encoding.UTF8.GetString(mso.ToArray());
    }
}