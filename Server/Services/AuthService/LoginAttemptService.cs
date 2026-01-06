using System.Collections.Concurrent;

namespace LolStatsTracker.API.Services.AuthService;

public interface ILoginAttemptService
{
    Task<bool> IsLockedOutAsync(string username);
    Task RecordFailedAttemptAsync(string username);
    Task ClearAttemptsAsync(string username);
    Task<int> GetRemainingAttemptsAsync(string username);
}

public class LoginAttemptService : ILoginAttemptService
{
    private readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();
    private readonly ILogger<LoginAttemptService> _logger;
    
    // Configuration
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AttemptWindowDuration = TimeSpan.FromMinutes(5);

    public LoginAttemptService(ILogger<LoginAttemptService> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsLockedOutAsync(string username)
    {
        var key = username.ToLowerInvariant();
        
        if (!_attempts.TryGetValue(key, out var info))
            return Task.FromResult(false);

        if (info.LockoutEnd.HasValue && info.LockoutEnd.Value > DateTime.UtcNow)
        {
            _logger.LogWarning("Account {Username} is locked out until {LockoutEnd}", 
                username, info.LockoutEnd.Value);
            return Task.FromResult(true);
        }

        // Lockout expired, clear it
        if (info.LockoutEnd.HasValue)
        {
            _attempts.TryRemove(key, out _);
        }

        return Task.FromResult(false);
    }

    public Task RecordFailedAttemptAsync(string username)
    {
        var key = username.ToLowerInvariant();
        var now = DateTime.UtcNow;

        _attempts.AddOrUpdate(key, 
            _ => new LoginAttemptInfo { FailedAttempts = 1, FirstAttemptTime = now },
            (_, existing) =>
            {
                // Reset if outside the attempt window
                if (existing.FirstAttemptTime.Add(AttemptWindowDuration) < now)
                {
                    return new LoginAttemptInfo { FailedAttempts = 1, FirstAttemptTime = now };
                }

                existing.FailedAttempts++;
                
                // Lock out if max attempts exceeded
                if (existing.FailedAttempts >= MaxAttempts)
                {
                    existing.LockoutEnd = now.Add(LockoutDuration);
                    _logger.LogWarning("Account {Username} has been locked out until {LockoutEnd} after {Attempts} failed attempts",
                        username, existing.LockoutEnd, existing.FailedAttempts);
                }

                return existing;
            });

        return Task.CompletedTask;
    }

    public Task ClearAttemptsAsync(string username)
    {
        var key = username.ToLowerInvariant();
        _attempts.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<int> GetRemainingAttemptsAsync(string username)
    {
        var key = username.ToLowerInvariant();
        
        if (!_attempts.TryGetValue(key, out var info))
            return Task.FromResult(MaxAttempts);

        // Check if outside the attempt window
        if (info.FirstAttemptTime.Add(AttemptWindowDuration) < DateTime.UtcNow)
            return Task.FromResult(MaxAttempts);

        return Task.FromResult(Math.Max(0, MaxAttempts - info.FailedAttempts));
    }

    private class LoginAttemptInfo
    {
        public int FailedAttempts { get; set; }
        public DateTime FirstAttemptTime { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
