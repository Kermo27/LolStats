using LolStatsTracker.Models;
using LolStatsTracker.Services;

public class MatchService
{
    private const string StorageKey = "matches";
    private readonly LocalStorageService _storage;
    private List<MatchEntry> _matches = new();

    public MatchService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        var loaded = await _storage.LoadAsync<List<MatchEntry>>(StorageKey);
        if (loaded != null)
            _matches = loaded;
    }

    public IEnumerable<MatchEntry> GetMatches() => _matches;

    public List<MatchEntry> GetAll() => _matches;

    public async Task AddMatchAsync(MatchEntry match)
    {
        // jeśli mecz nie ma Id, przypisz nowy Guid
        if (match.Id == Guid.Empty)
            match.Id = Guid.NewGuid();

        _matches.Add(match);
        await _storage.SaveAsync(StorageKey, _matches);
    }

    public async Task UpdateMatchAsync(MatchEntry match)
    {
        var index = _matches.FindIndex(m => m.Id == match.Id);
        if (index >= 0)
            _matches[index] = match;

        await _storage.SaveAsync(StorageKey, _matches);
    }

    public async Task RemoveMatchAsync(MatchEntry match)
    {
        _matches.RemoveAll(m => m.Id == match.Id);
        await _storage.SaveAsync(StorageKey, _matches);
    }

    public async Task ClearAsync()
    {
        _matches.Clear();
        await _storage.RemoveAsync(StorageKey);
    }

    public IEnumerable<MatchStats> GetChampionStats()
    {
        return _matches
            .GroupBy(m => m.Champion)
            .Select(g => new MatchStats
            {
                Champion = g.Key,
                Games = g.Count(),
                Wins = g.Count(m => m.Win),
                WinRate = Math.Round(100.0 * g.Count(m => m.Win) / g.Count(), 1),
                AvgKda = Math.Round(g.Average(m => (m.Kills + m.Assists) / Math.Max(1.0, m.Deaths)), 2),
                AvgCsm = Math.Round(g.Average(m => m.Cs / Math.Max(1.0, m.GameLengthMinutes)), 2)
            })
            .OrderByDescending(s => s.WinRate);
    }
}
