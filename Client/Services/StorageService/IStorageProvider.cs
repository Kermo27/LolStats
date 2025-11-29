namespace LolStatsTracker.Services;

public interface IStorageProvider
{
    Task SaveAsync<T>(string key, T data);
    Task<T?> LoadAsync<T>(string key);
    Task RemoveAsync(string key);   
}